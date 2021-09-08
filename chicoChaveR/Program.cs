using System;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace chicoChaveR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Digite a UF do emitente: 02 digitos");
            string uf = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite data de emissao: ( AAMM ) 04 digitos");
            string dEmi = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite o cnpj do Emitente 14 digitos ou 00 + 9 CPF ");
            string cnpj = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite o modelo do documento: 02 digitos ");
            string mod = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite a serie do documento: 03 digitos ");
            string serie = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite o numero do documento: 09 digitos ");
            string nDoc = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite o tpEmis do documento: 01 digito ");
            string tpEmis = Console.ReadLine();
            Console.WriteLine("");

            Console.WriteLine("Digite o tpAmb do documento: 01 digito ");
            string tpAmb = Console.ReadLine();
            Console.WriteLine("");

            string cNF = gerarCodigoCDF().ToString();

            string chave43 = uf + dEmi + cnpj + mod + serie + nDoc + tpEmis + cNF;

            string chave44 = chave43 + GerarDigitoVerificador(chave43).ToString();

            ConsSitReq ConsSitReq = new ConsSitReq
            {
                tpAmb = tpAmb,
                chNFe = chave44,
                licencaCnpj = cnpj,
                versao = "4.00",
            };

            //Console.WriteLine("Chave de acesso gerada pelo Chico: \n");
            //Console.WriteLine(chave44);

            if (mod == "55")
            {
                string retorno = consultarSituacaoDocumento("55", ConsSitReq);
                dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
                dynamic retConsSitNFe = respostaJson.retConsSitNFe;
                string xMotivo = retConsSitNFe.xMotivo;
                string chaveEncontrada = new String(xMotivo.Where(Char.IsDigit).ToArray());

                if (chaveEncontrada != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada); }
                else { Console.WriteLine("Chico ChaveR não encontrou a chave: " + xMotivo); }

                Console.ReadKey();
            }
            else
            {
                string retorno = consultarSituacaoDocumento("65", ConsSitReq);
                dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
                dynamic nfeProc = respostaJson.nfeProc;
                string xMotivo = nfeProc.xMotivo;
                string chaveEncontrada = new String(xMotivo.Where(Char.IsDigit).ToArray());

                if (chaveEncontrada != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada); }
                else { Console.WriteLine("Chico ChaveR não encontrou a chave: " + xMotivo); }

                Console.ReadKey();
            }
        }

        public static int gerarCodigoCDF()
        {
            int min = 10000000;
            int max = 99999999;
            Random random = new Random();
            return random.Next(min, max);
        }

        public class ConsSitReq
        {
            public string licencaCnpj { get; set; }
            public string tpAmb { get; set; }
            public string chNFe { get; set; }
            public string versao { get; set; }
        }

        public class NSSuite
        {

            public static string enviaConteudoParaAPI(string conteudo, string url, string tpConteudo)
            {
                string retorno = "";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebRequest.Method = "POST";

                Console.WriteLine("Cole aqui o token da Software House: ");
                string token = Console.ReadLine();

                httpWebRequest.Headers["X-AUTH-TOKEN"] = token;

                httpWebRequest.ContentType = "application/json;charset=utf-8";
   
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(conteudo);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        retorno = streamReader.ReadToEnd();
                    }
                }

                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)ex.Response;

                        using (var streamReader = new StreamReader(response.GetResponseStream()))
                        {
                            retorno = streamReader.ReadToEnd();
                        }

                        switch (Convert.ToInt32(response.StatusCode))
                        {
                            case 401:
                                {
                                    Console.WriteLine("Token nao enviado ou invalido");
                                    break;
                                }

                            case 403:
                                {
                                    Console.WriteLine("Token sem permissao");
                                    break;
                                }

                            case 404:
                                {
                                    Console.WriteLine("Nao encontrado, verifique o retorno para mais informacoes");
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }
                    }
                }

                return retorno;
            }

        }

        public static string consultarSituacaoDocumento(string modelo, ConsSitReq ConsSitReq)
        {
            string urlConsSit = "";

            switch (modelo)
            {
                case "55":
                    urlConsSit = "https://nfe.ns.eti.br/nfe/stats";
                break;

                case "65":
                    urlConsSit = "https://nfce.ns.eti.br/v1/nfce/status";
                break;
            }

            string json = JsonConvert.SerializeObject(ConsSitReq);

            string resposta = NSSuite.enviaConteudoParaAPI(json, urlConsSit, "json");

            return resposta;
        }
        private static int GerarDigitoVerificador(string chave43)
        {
            int soma = 0;
            int restoDivisao = -1;
            int digitoVerificador = -1;
            int pesoMultiplicacao = 2;

            for (int i = chave43.Length - 1; i != -1; i--)
            {
                int ch = Convert.ToInt32(chave43[i].ToString());
                soma += ch * pesoMultiplicacao;

                if (pesoMultiplicacao < 9)
                    pesoMultiplicacao += 1;
                else
                    pesoMultiplicacao = 2;
            }

            restoDivisao = soma % 11;

            if (restoDivisao == 0 || restoDivisao == 1)
                digitoVerificador = 0;

            else
                digitoVerificador = 11 - restoDivisao;

            return digitoVerificador;
        }

    }
}
