using Microsoft.AspNetCore.Mvc;
using Rota.Application.DTOs;
using Rota.Application.Services;
using Swashbuckle.AspNetCore.Annotations; 
using Rota.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Rota.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador, Financeiro")]
    public class CommissionRulesController : ControllerBase
    {
        private readonly CommissionRuleService _service;

        public CommissionRulesController(CommissionRuleService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lista todas as regras cadastradas")]
        public async Task<IActionResult> GetAll()
        {
            var rules = await _service.GetAllAsync();
            return Ok(rules);
        }

        [HttpPost("preview")]
        [SwaggerOperation(
            Summary = "Simula e valida uma fórmula de comissão personalizada",
            Description = @"
            ### 📘 Guia de Uso do Motor de Fórmulas

            Este endpoint serve para testar se a sua lógica matemática está correta.
            
            ⚠️ **IMPORTANTE SOBRE DEDUÇÕES:**
            O sistema **subtrai automaticamente** o valor das Notas Fiscais (`DED`) após o cálculo da sua fórmula.
            * Se você quer o valor líquido padrão, sua fórmula final deve retornar apenas o valor bruto.
            * Se você colocar `- DED` na fórmula, o desconto será aplicado **duas vezes**.

            ---

            ### 📝 Dicionário de Variáveis Disponíveis

            **1. Valores das Transações (Vindos do banco):**
            * `VP`: Total de **P**assagens
            * `VL`: Total de **L**inks (Internet)
            * `VS`: Total de **S**eguros
            * `VV`: Total de **V**olume Especial
            * `VE`: Total de **E**ncomendas

            **2. Taxas da Agência (Configuradas no cadastro):**
            * `TP`: Taxa de Passagem (ex: 0.10)
            * `TL`: Taxa de Link
            * `TS`: Taxa de Seguro
            * `TV`: Taxa de Volume
            * `TE`: Taxa de Encomenda

            **3. Variáveis Especiais:**
            * `DED`: Total de Deduções (Notas Fiscais). **(Use com cuidado, pois já é descontado no final)**
            * `TOTAL`: Resultado da 1ª fórmula (`TotalSalesFormula`).

            ---

            ### 💡 Exemplo Prático: O Cálculo Padrão

            Para reproduzir a lógica padrão (Comissão Bruta - Notas):

            **Campo totalSalesFormula:**
            `(VP * TP) + (VL * TL) + (VS * TS) + (VV * TV) + (VE * TE)`

            **Campo commissionFormula:**
            `TOTAL`

            *(Nota: Como a fórmula retorna `TOTAL`, o sistema fará automaticamente `TOTAL - DED` para salvar no banco).*

            ---

            ### 🧪 Testando Cenários
            Você pode preencher os campos `testVP`, `testVL`, etc., para simular valores específicos.
            "
        )]
        public IActionResult Preview([FromBody] RulePreviewRequestDTO dto)
        {
            var result = _service.Preview(dto);
            
            if (!result.Success)
                return BadRequest(new { error = "Fórmula inválida ou erro matemático", details = result.ErrorMessage });

            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cria e salva uma nova regra de comissão",
            Description = "Salva a regra no banco de dados. **Atenção:** Este endpoint roda a mesma validação do /preview internamente. Se a fórmula estiver errada, o salvamento será bloqueado."
        )]
        public async Task<IActionResult> Create([FromBody] RuleCreateDTO dto)
        {
            try 
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Regra criada com sucesso." });
            }
            catch(Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remove uma regra de comissão")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {

                
                await _service.DeleteAsync(id); 
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "Regra não encontrada." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}