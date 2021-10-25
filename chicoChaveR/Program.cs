using System;
using System.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;

namespace chicoChaveR
{
    class Program
    {
        static void Main(string[] args) {

            Console.WriteLine("1: adivinhar 1 chave");

            Console.WriteLine("2: adivinhar lista de chaves");

            string opcao = Console.ReadLine();

            Console.Clear();

            if (opcao == "1")
            {
                Console.WriteLine("sh_token: ");
                string token = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("digite chave de referencia: ");
                string chaveReferencia = Console.ReadLine();
                adivinharChave(chaveReferencia, token);

                Thread.Sleep(15000);
            }
            if (opcao == "2")
            {
                Console.WriteLine("sh_token: ");
                string token = Console.ReadLine();
                Console.Clear();

                adivinharChaves(token);

                Thread.Sleep(15000);
            }
        }

        static void adivinharChaves(string sh_token)
        {
            string[] chaves = lerArquivoChaves();

            foreach (string n in chaves)
            {
                string chavesEncontrada = adivinharChave(n, sh_token, true);
                Thread.Sleep(400);
            }

        }

        static string adivinharChave(string chaveInformada, string sh_token, bool gravarRetorno = false)
        {
            string chaveNormal = verificarEmissaoNormal(chaveInformada, sh_token, gravarRetorno);

            string chaveContingencia = verificarEmissaoContingencia(chaveInformada, sh_token, gravarRetorno);

            string chaveEncontrada = "";

            if (chaveNormal != "") {

                chaveEncontrada = chaveNormal;

                Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada);

                if (gravarRetorno)
                {
                    dynamic situacao = obterSituacao(chaveEncontrada, sh_token);

                    DateTime dataEmissao = Convert.ToDateTime(situacao.dhRecbto);
                    string dataLog = dataEmissao.ToShortDateString().ToString().Replace("/", "");

                    // criar um diretorio com a data da emissao
                    criarDiretorios(dataLog);

                    // salvar um arquivo de chaves a recuperar
                    gerarArquivoChavesRecuperar(dataLog, chaveEncontrada);

                    // salvar um arquivo com a situacao delas
                    gerarArquivoSituacao(dataLog, chaveEncontrada, situacao);
                }
            }

            else if (chaveContingencia != "")
            {
                chaveEncontrada = chaveContingencia;

                Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada);

                if (gravarRetorno)
                {
                    dynamic situacao = obterSituacao(chaveEncontrada, sh_token);
                    DateTime dataEmissao = Convert.ToDateTime(situacao.dhRecbto);
                    string dataLog = dataEmissao.ToShortDateString().ToString().Replace("/", "");


                    // criar um diretorio com a data da emissao
                    criarDiretorios(dataLog);

                    // salvar um arquivo de chaves a recuperar
                    gerarArquivoChavesRecuperar(dataLog, chaveEncontrada);

                    // salvar um arquivo com a situacao delas
                    gerarArquivoSituacao(dataLog, chaveEncontrada, situacao);

                }
            }

            else
            {
                Console.WriteLine("Chico ChaveR nao encontrou a chave: " + chaveInformada);
                if (gravarRetorno)
                {
                    // salvar um arquivo com as chaves nao encontradas
                    gerarArquivoNaoAutorizadas(chaveInformada);
                }
            }

            return chaveEncontrada;
        }

        private static string verificarEmissaoNormal(string chave, string sh_token, bool gravarRetorno = false)
        {
            string chave_verificar = chave.Remove(chave.Length - 9, 9);
            chave_verificar = chave_verificar.Remove(chave_verificar.Length - 1, 1);
            chave_verificar = chave_verificar + "1";
            chave_verificar = chave_verificar + gerarCodigoCDF();
            chave_verificar = chave_verificar + GerarDigitoVerificador(chave_verificar);

            string chaveEmissaoNormal = obterChave(chave_verificar, sh_token, gravarRetorno);
            return chaveEmissaoNormal;

        }

        private static string verificarEmissaoContingencia(string chave, string sh_token, bool gravarRetorno = false)
        {
            string chave_verificar = chave.Remove(chave.Length - 9, 9);
            chave_verificar = chave_verificar.Remove(chave_verificar.Length - 1, 1);
            chave_verificar = chave_verificar + "9";
            chave_verificar = chave_verificar + gerarCodigoCDF();
            chave_verificar = chave_verificar + GerarDigitoVerificador(chave_verificar);

            string chaveContingencia = obterChave(chave_verificar, sh_token, gravarRetorno);
            return chaveContingencia;
        }

        private static string obterChave(string chave, string sh_token, bool gravarRetorno = false)
        {
            string cnpj = getCNPJ(chave);

            ConsSitReq consultarSituacao = new ConsSitReq
            {
                tpAmb = "1",
                chNFe = chave,
                licencaCnpj = cnpj,
                versao = "4.00",
            };

            string retorno = consultarSituacaoDocumento(obterMod(consultarSituacao.chNFe), consultarSituacao, sh_token);

            dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
            dynamic nfeProc = respostaJson.nfeProc;
            string xMotivo = nfeProc.xMotivo;
            string chaveEncontrada = new String(xMotivo.Where(Char.IsDigit).ToArray());

            return chaveEncontrada;
        }

        private static dynamic obterSituacao(string chave, string sh_token)
        {
            string cnpj = getCNPJ(chave);

            ConsSitReq consultarSituacao = new ConsSitReq
            {
                tpAmb = "1",
                chNFe = chave,
                licencaCnpj = cnpj,
                versao = "4.00",
            };

            string retorno = consultarSituacaoDocumento(obterMod(consultarSituacao.chNFe), consultarSituacao, sh_token);

            dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
            dynamic nfeProc = respostaJson.nfeProc;

            return nfeProc;
        }

        public static int gerarCodigoCDF()
        {
            int min = 10000000;
            int max = 99999999;
            Random random = new Random();
            return random.Next(min, max);
        }

        public static string consultarSituacaoDocumento(string modelo, ConsSitReq ConsSitReq, string sh_token)
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

            string resposta = NSSuite.enviaConteudoParaAPI(json, urlConsSit, "json", sh_token);

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

        private static string[] lerArquivoChaves()
        {
            try
            {
                string[] chaves = File.ReadAllLines(@"./chaves.txt");
                return chaves;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            
        }

        private static string gerarNovaChave(string chave)
        {
            string novaChave = chave.Remove(chave.Length - 9, 9);

            novaChave = novaChave + gerarCodigoCDF();
            novaChave = novaChave + GerarDigitoVerificador(chave);

            return novaChave;        }

        private static string getCNPJ(string chave)
        {
            string cnpj = chave.Remove(chave.Length - 24, 24);
            cnpj = cnpj.Remove(0, 6);

            return cnpj;
        }

        private static string obterMod(string chave)
        {
            string mod = chave.Remove(chave.Length - 22, 22);
            mod = mod.Remove(0, 20);

            return mod;
        }

        private static void criarDiretorios(string dataLog)
        {
            string caminho = @".\ret\"+ dataLog;

            if (!Directory.Exists(caminho))
                Directory.CreateDirectory(caminho);
        }

        private static void gerarArquivoChavesRecuperar(string dataLog, string chaveEncontrada)
        {
            using (StreamWriter outputFile = new StreamWriter(@"./ret/"+ dataLog + @"\" + "chaves_recuperar" + ".txt", true))
            {
                outputFile.WriteLine(chaveEncontrada);
            }
        }

        private static void gerarArquivoSituacao(string dataLog, string chaveEncontrada, dynamic situacao)
        {
            using (StreamWriter outputFile = new StreamWriter(@"./ret/" + dataLog + @"\" + "situacao_autorizadas" + ".txt", true))
            {
                string conteudo = chaveEncontrada + " : " + situacao;
                outputFile.WriteLine(conteudo.Replace("\n", "").Replace("\r", ""));
            }
        }

        private static void gerarArquivoNaoAutorizadas(string chaveInformada)
        {
            using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "nao_autorizadas" + ".txt", true))
            {
                outputFile.WriteLine(chaveInformada);
            }
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
            public static string enviaConteudoParaAPI(string conteudo, string url, string tpConteudo, string sh_token)
            {
                string retorno = "";

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                httpWebRequest.Method = "POST";

                httpWebRequest.Headers["X-AUTH-TOKEN"] = sh_token;

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
    }
}
