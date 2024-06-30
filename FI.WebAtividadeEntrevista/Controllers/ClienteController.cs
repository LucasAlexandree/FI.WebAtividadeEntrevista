using FI.AtividadeEntrevista.BLL;
using FI.AtividadeEntrevista.DML;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAtividadeEntrevista.Models;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            BoCliente bo = new BoCliente();
            BoValidarCPF boValidarCPF = new BoValidarCPF();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (!boValidarCPF.ValidaCPF(model.CPF)) //Verifica o cpf conforme calculo dos numeros
                {
                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, "Este não é um CPF valido!"));
                }

                if (bo.VerificarExistencia(model.CPF)) //cpf já cadastrado no banco de dados
                {
                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, "Este CPF ja esta em uso!"));
                }

                model.Id = bo.Incluir(new Cliente()
                {
                    CEP = model.CEP,
                    CPF = model.CPF,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone
                });

                if (!string.IsNullOrEmpty(model.Beneficiarios))
                {
                    List<BeneficiarioModel> beneficiarios = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(model.Beneficiarios);

                    var result = ManipularBeneficiarios(beneficiarios, model.Id); //Aqui ele leva ao metodo do beneficiario, onde verifica se ocorreu alteração 

                    if (!string.IsNullOrEmpty(result))
                    {
                        Response.StatusCode = 400;
                        return Json(string.Join(Environment.NewLine, result));
                    }
                }

                return Json("Cadastro efetuado com sucesso");
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();
            BoValidarCPF boValidarCPF = new BoValidarCPF();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (!boValidarCPF.ValidaCPF(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, "Este não é um CPF valido!"));
                }

                Cliente cliente = bo.Consultar(model.Id);

                if (cliente.CPF != model.CPF)
                {
                    if (bo.VerificarExistencia(model.CPF))
                    {
                        Response.StatusCode = 400;
                        return Json(string.Join(Environment.NewLine, "Este CPF ja esta em uso!"));
                    }
                }

                bo.Alterar(new Cliente()
                {
                    Id = model.Id,
                    CEP = model.CEP,
                    CPF = model.CPF,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone
                });

                if (!string.IsNullOrEmpty(model.Beneficiarios))
                {
                    List<BeneficiarioModel> beneficiarios = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(model.Beneficiarios);

                    var result = ManipularBeneficiarios(beneficiarios, model.Id);

                    if (!string.IsNullOrEmpty(result))
                    {
                        Response.StatusCode = 400;
                        return Json(string.Join(Environment.NewLine, result));
                    }
                }

                return Json("Cadastro alterado com sucesso");
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            List<Beneficiario> beneficiario = new BoBeneficiario().Pesquisar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    CPF = cliente.CPF,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    Beneficiarios = beneficiario == null ? "" : JsonConvert.SerializeObject(beneficiario)
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult Excluir(ClienteModel model)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();
            BoValidarCPF boValidarCPF = new BoValidarCPF();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                bo.Excluir(model.Id);

                if (!string.IsNullOrEmpty(model.Beneficiarios))
                {
                    List<BeneficiarioModel> beneficiarios = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(model.Beneficiarios);

                    var result = ManipularBeneficiarios(beneficiarios, model.Id);

                    if (!string.IsNullOrEmpty(result))
                    {
                        Response.StatusCode = 400;
                        return Json(string.Join(Environment.NewLine, result));
                    }
                }

                return Json("Cadastro alterado com sucesso");
            }
        }

        [HttpGet]
        public ActionResult Excluir(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            List<Beneficiario> beneficiario = new BoBeneficiario().Pesquisar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    CPF = cliente.CPF,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    Beneficiarios = beneficiario == null ? "" : JsonConvert.SerializeObject(beneficiario)
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        public string ManipularBeneficiarios(List<BeneficiarioModel> beneficiarios, long clienteId)
        {
            BoBeneficiario boBeneficiario = new BoBeneficiario();
            BoValidarCPF boValidarCPF = new BoValidarCPF();
            List<BeneficiarioModel> beneficiarioAlterado = new List<BeneficiarioModel>();
            List<long> beneficiariosModelIds = new List<long>();
            List<long> beneficiariosDelIds = new List<long>();
            List<long> beneficiariosIds = new List<long>();

            List<Beneficiario> listBeneficiarios = new BoBeneficiario().Pesquisar(clienteId);

            beneficiarios.ForEach(item => beneficiariosModelIds.Add(item.Id));
            beneficiarioAlterado = beneficiarios.Where(w => w.Alterado).ToList(); //alteração

            if (listBeneficiarios.Count > 0)
            {
                beneficiariosDelIds = listBeneficiarios.Where(w => !beneficiariosModelIds.Contains(w.Id)).Select(x => x.Id).ToList(); //exclusão
                beneficiariosIds = listBeneficiarios.Where(w => beneficiariosModelIds.Contains(w.Id)).Select(x => x.Id).ToList(); //inclusão
            }

            if (beneficiariosDelIds.Count > 0)
                beneficiariosDelIds.ForEach(id => boBeneficiario.Excluir(id));

            if (beneficiarios.Count > 0)
            {
                beneficiarioAlterado.ForEach(
                    beneficiario => boBeneficiario.Alterar(
                        new Beneficiario()
                        {
                            Id = beneficiario.Id,
                            IdCliente = beneficiario.IdCliente,
                            Nome = beneficiario.Nome,
                            CPF = beneficiario.CPF
                        }
                    )
                );
            }

            foreach (var beneficiario in beneficiarios)
            {
                if (!beneficiariosIds.Contains(beneficiario.Id)) //alem da verificação por cpf, verifica também por id
                {
                    if (!boValidarCPF.ValidaCPF(beneficiario.CPF))
                    {
                        return $"Este não é um CPF valido: {beneficiario.CPF}";
                    }

                    if (boBeneficiario.VerificarExistencia(beneficiario.CPF))
                    {
                        return $"Este CPF ja esta em uso: {beneficiario.CPF}";
                    }

                    beneficiario.Id = boBeneficiario.Incluir(new Beneficiario()
                    {
                        IdCliente = clienteId,
                        Nome = beneficiario.Nome,
                        CPF = beneficiario.CPF
                    });
                }
            }

            return "";
        }

        public ActionResult ModalBeneficiario()
        {
            return PartialView();
        }
    }
}