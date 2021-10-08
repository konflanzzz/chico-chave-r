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

                adivinharChaves(token);

                Thread.Sleep(15000);
            }
        }
        static string adivinharChave(string chaveInformada, string sh_token, bool gravarRetorno = false)
        {
            string chaveNormal = verificarEmissaoNormal(chaveInformada, sh_token, gravarRetorno);

            string chaveContingencia = verificarEmissaoContingencia(chaveInformada, sh_token, gravarRetorno);

            string chaveEncontrada = "";

            if (chaveNormal != "") {

                chaveEncontrada = chaveNormal;

                if (chaveEncontrada != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada); }
            }

            else if (chaveContingencia != "") {

                chaveEncontrada = chaveContingencia;

                if (chaveEncontrada != "") { 
                    Console.WriteLine("Chico ChaveR descobriu a chave emitida em contingencia: " + chaveEncontrada); 
                }
            }

            else
            {
                if (chaveEncontrada == "") { Console.WriteLine("Chico ChaveR não encontrou este documento"); }
            }

            return chaveEncontrada;
        }
        static void adivinharChaves(string sh_token)
        {
            string[] chaves = lerArquivoChaves();

            foreach(string n in chaves)
            {
                string chavesEncontrada = adivinharChave(n, sh_token, true);
                Thread.Sleep(400);
            }

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

        //private static void superChico(string[] chaves, string token)
        //{
        //    for(int i = 0; i <= chaves.Length; i++)
        //    {
        //        string chave = chaves[i].Remove(chaves[i].Length-9, 9);

        //        chave = chave + gerarCodigoCDF();
        //        chave = chave + GerarDigitoVerificador(chave);

        //        string cnpj = chaves[i].Remove(chaves[i].Length - 24, 24);
        //        cnpj = cnpj.Remove(0, 6);

        //        ConsSitReq ConsSitReq = new ConsSitReq
        //        {
        //            tpAmb = "1",
        //            chNFe = chave,
        //            licencaCnpj = cnpj,
        //            versao = "4.00",
        //        };

        //        Thread.Sleep(500);
        //        string retorno = consultarSituacaoDocumento("65", ConsSitReq, token);
                
        //        dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
        //        dynamic nfeProc = respostaJson.nfeProc;
        //        string xMotivo = nfeProc.xMotivo;
        //        string chaveEncontrada = new String(xMotivo.Where(Char.IsDigit).ToArray());

        //        if (chaveEncontrada != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chaveEncontrada); }
        //        else { Console.WriteLine("Chico ChaveR não encontrou a chave "+ chave + ": " + xMotivo); }

        //        if (chaveEncontrada != "")
        //        {
        //            ConsSitReq segundaReq = new ConsSitReq
        //            {
        //                tpAmb = "1",
        //                chNFe = chaveEncontrada,
        //                licencaCnpj = cnpj,
        //                versao = "4.00",
        //            };

        //            Thread.Sleep(500);
        //            string segundoRetorno = consultarSituacaoDocumento("65", segundaReq, token);

        //            dynamic respostaJson2 = JsonConvert.DeserializeObject(segundoRetorno);
        //            dynamic nfeProc2 = respostaJson2.nfeProc;
        //            string status = respostaJson2.status; if (status == "-997")
        //            {
        //                using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "retorno" + ".txt", true))
        //                {
        //                    outputFile.WriteLine(chaveEncontrada + " : " + segundoRetorno);
        //                }
        //            }
        //            if (status != "-997")
        //            {
        //                string cStat = nfeProc2.cStat;
        //                if (cStat == "217")
        //                {
        //                    string chave_verificar = chaves[i].Remove(chaves[i].Length - 9, 9);

        //                    if (chave_verificar.EndsWith("9"))
        //                    {
        //                        chave_verificar = chave_verificar.Remove(chave_verificar.Length - 1, 1);
        //                        chave_verificar = chave_verificar + "1";
        //                        chave_verificar = chave_verificar + gerarCodigoCDF();
        //                        chave_verificar = chave_verificar + GerarDigitoVerificador(chave_verificar);

        //                        ConsSitReq verificar_tp_emis = new ConsSitReq
        //                        {
        //                            tpAmb = "1",
        //                            chNFe = chave_verificar,
        //                            licencaCnpj = cnpj,
        //                            versao = "4.00",
        //                        };

        //                        Thread.Sleep(500);
        //                        string retorno_verificar = consultarSituacaoDocumento("65", verificar_tp_emis, token);
        //                        dynamic respostaJson_verificar = JsonConvert.DeserializeObject(retorno_verificar);
        //                        dynamic nfeProc_verificar = respostaJson_verificar.nfeProc;

        //                        string xMotivo_verificar = nfeProc_verificar.xMotivo;
        //                        string chave_reconsultar = new String(xMotivo_verificar.Where(Char.IsDigit).ToArray());

        //                        if (chave_reconsultar != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chave_reconsultar); }
        //                        else { Console.WriteLine("Chico ChaveR não encontrou a chave: " + nfeProc_verificar.xMotivo); }

        //                        ConsSitReq reconsulta = new ConsSitReq
        //                        {
        //                            tpAmb = "1",
        //                            chNFe = chave_reconsultar,
        //                            licencaCnpj = cnpj,
        //                            versao = "4.00",
        //                        };

        //                        Thread.Sleep(500);
        //                        string retorno_reconsulta = consultarSituacaoDocumento("65", reconsulta, token);

        //                        string cStat_verificar = nfeProc_verificar.cStat;
        //                        string status_verificar = respostaJson_verificar.status;

        //                        using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "retorno" + ".txt", true))
        //                        {
        //                            outputFile.WriteLine(chave_reconsultar + " : " + retorno_reconsulta);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        chave_verificar = chave_verificar.Remove(chave_verificar.Length - 1, 1);
        //                        chave_verificar = chave_verificar + "9";
        //                        chave_verificar = chave_verificar + gerarCodigoCDF();
        //                        chave_verificar = chave_verificar + GerarDigitoVerificador(chave_verificar);

        //                        ConsSitReq verificar_tp_emis = new ConsSitReq
        //                        {
        //                            tpAmb = "1",
        //                            chNFe = chave_verificar,
        //                            licencaCnpj = cnpj,
        //                            versao = "4.00",
        //                        };

        //                        Thread.Sleep(500);
        //                        string retorno_verificar = consultarSituacaoDocumento("65", verificar_tp_emis, token);
        //                        dynamic respostaJson_verificar = JsonConvert.DeserializeObject(retorno_verificar);
        //                        dynamic nfeProc_verificar = respostaJson_verificar.nfeProc;

        //                        string xMotivo_verificar = nfeProc_verificar.xMotivo;
        //                        string chave_reconsultar = new String(xMotivo_verificar.Where(Char.IsDigit).ToArray());

        //                        if (chave_reconsultar != "") { Console.WriteLine("Chico ChaveR descobriu a chave: " + chave_reconsultar); }
        //                        else { Console.WriteLine("Chico ChaveR não encontrou a chave: " + nfeProc_verificar.xMotivo); }

        //                        ConsSitReq reconsulta = new ConsSitReq
        //                        {
        //                            tpAmb = "1",
        //                            chNFe = chave_reconsultar,
        //                            licencaCnpj = cnpj,
        //                            versao = "4.00",
        //                        };

        //                        Thread.Sleep(500);
        //                        string retorno_reconsulta = consultarSituacaoDocumento("65", reconsulta, token);

        //                        string cStat_verificar = nfeProc_verificar.cStat;
        //                        string status_verificar = respostaJson_verificar.status;


        //                        using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "retorno" + ".txt", true))
        //                        {
        //                            outputFile.WriteLine(chave_reconsultar + " : " + retorno_reconsulta);
        //                        }

        //                    }
        //                }

        //                else
        //                {
        //                    using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "retorno" + ".txt", true))
        //                    {
        //                        outputFile.WriteLine(nfeProc2.chNFe + " : " + segundoRetorno);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (StreamWriter outputFile = new StreamWriter(@"./ret/" + "retorno" + ".txt", true))
        //            {
        //                outputFile.WriteLine(chave + " : " + xMotivo);
        //            }
        //        }
        //    }
        //}

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

            string retorno = consultarSituacaoDocumento("65", consultarSituacao, sh_token);

            dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
            dynamic nfeProc = respostaJson.nfeProc;
            string xMotivo = nfeProc.xMotivo;
            string chaveEncontrada = new String(xMotivo.Where(Char.IsDigit).ToArray());

            dynamic situacao = "";

            if (chaveEncontrada != "")
            {
                situacao = obterSituacao(chaveEncontrada, sh_token);

                if (gravarRetorno)
                {
                    using (StreamWriter outputFile = new StreamWriter(@"./" + "consultas_"+ cnpj + ".txt", true))
                    {
                        outputFile.WriteLine(chaveEncontrada + " : " + situacao);
                    }
                }
            }

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

            string retorno = consultarSituacaoDocumento("65", consultarSituacao, sh_token);

            dynamic respostaJson = JsonConvert.DeserializeObject(retorno);
            dynamic nfeProc = respostaJson.nfeProc;

            return nfeProc;
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
