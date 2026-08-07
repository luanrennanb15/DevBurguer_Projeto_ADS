# Refatoração do DevBurguer — Plano / Roadmap

Documento de arquitetura e plano de trabalho para deixar o DevBurguer mais
organizado, profissional e pronto para deploy. Vai sendo atualizado conforme
avançamos.

---

## Objetivo

Sair de um sistema que **funciona mas está acoplado** para um sistema
**separado em camadas, testável e publicável na internet**, sem parar de
funcionar em nenhum momento (evolução incremental, nada de reescrever do zero).

## Princípios

- **Uma responsabilidade por arquivo/camada.** Tela cuida da tela; repositório
  cuida do banco; serviço cuida da regra.
- **Dependência aponta pra dentro.** UI depende de Serviços/Dados; o inverso
  nunca acontece.
- **Cada passo deixa o sistema compilando e rodando.** Refatora um pedaço,
  testa, segue.
- **Sem SQL solto na interface.** Todo acesso a banco passa por repositório.

---

## Diagnóstico atual (junho/2026)

O que já está bom:

- Existe uma base de camadas: `Data/` (repositórios), `Interfaces/`, `Models/`,
  `Services/`, `Banco/Conexao`, `DbHelper`.
- A API Node e o site estão bem organizados.

O que precisa melhorar:

- **8 telas falam SQL direto com o banco**, furando os repositórios:
  FormClientes, FormDashboard, FormMotoboy, FormEscalaMotoboy, FormPrevisao,
  FormFaturamentoMotoboy, FormConfiguracoes, FormLogin.
- **Repositórios desatualizados** — ex.: `ClienteRepository.InsertAsync` nem
  tinha `Numero`/`Bairro`, por isso a tela o ignorou. Sintoma clássico de
  código que "envelheceu" sem ser mantido junto.
- **Telas gigantes** misturando montagem de UI + regra + dados (FormProducao
  tem 849 linhas).
- **Dois back-ends para o mesmo banco:** o site vai pela API; o desktop vai
  direto no SQL. Falta um back-end único.
- Configuração e string de conexão presas ao `localhost`.

---

## Arquitetura alvo (visão geral, em fases)

### Fase 1 — Limpeza do desktop  ← **ESTAMOS AQUI**
Deixar o desktop coeso, sem SQL nas telas, em camadas de verdade. Baixo risco,
é a fundação pro resto.

### Fase 2 — Deploy da web
Banco na nuvem, API hospedada, site publicado. Config por ambiente, CORS
travado, autenticação na API. Aprende deploy sem mexer muito no desktop.

### Fase 3 — Back-end único
Expandir a API para cobrir tudo que o desktop faz e transformar o desktop em
**cliente da API** (HttpClient) em vez de acessar o SQL direto. Aí o desktop
roda de qualquer lugar e existe só um back-end.

### Fase 4 — Profissionalização
Autenticação com token (JWT), hash de senha decente (PBKDF2), validação,
logging estruturado, testes automatizados e CI/CD.

---

## Fase 1 em detalhe — Limpeza do desktop

### 1.1 Tirar o SQL das telas → repositórios
Cada tela com SQL inline passa a chamar um repositório. Ordem sugerida
(da mais simples para a mais complexa):

- [x] **FormClientes** → `ClienteRepository` *(feito — caso-modelo do padrão)*
- [x] **FormLogin** → `UsuarioRepository` + `SecurityHelper` *(feito)*
- [ ] FormConfiguracoes → `ConfiguracaoRepository`
- [ ] FormMotoboy → `MotoboyRepository` (já existe, completar)
- [ ] FormEscalaMotoboy → `EscalaMotoboyRepository`
- [ ] FormDashboard → `DashboardRepository`/`RelatorioService`
- [ ] FormFaturamentoMotoboy → `RelatorioService`
- [ ] FormPrevisao → `PrevisaoRepository` (só a busca de dados; o cálculo da
      regressão linear fica num serviço `PrevisaoService`)

### 1.2 Padrão de repositório (convenção)
- Métodos `async`, retornam dados (DataTable ou Model), nunca mexem em UI.
- Usam `DbHelper` para executar (nada de abrir conexão na mão).
- Tratamento de erro: **logar e relançar** (`catch { Log; throw; }`), pra a
  tela decidir a mensagem ao usuário.
- SQL sempre parametrizado.

### 1.3 Separar em camadas/projetos
Depois que as telas estiverem limpas, organizar em projetos (assemblies) para o
compilador impedir dependência errada:
- `DevBurguer.Core` — Models, Interfaces, regras puras.
- `DevBurguer.Data` — repositórios, DbHelper, conexão.
- `DevBurguer.Services` — serviços (previsão, impressão, som, etc.).
- `DevBurguer.UI` — os Forms.
(Feito por último na fase, porque mexe em `.csproj` e referências.)

### 1.4 Injeção de dependência leve
Trocar `new XRepository()` espalhado por injeção via construtor, deixando as
telas testáveis. Opcional nesta fase; recomendado antes da Fase 3.

---

## Convenções gerais

- **Nomes:** repositórios terminam em `Repository`; serviços em `Service`.
- **Erro:** camada de dados loga e relança; camada de UI captura e mostra
  mensagem amigável via `DialogHelper`.
- **Nada de `catch` vazio.**
- **Um commit por passo** (por tela refatorada), pra poder voltar fácil.

---

## Progresso

| Data | Passo | Status |
|------|-------|--------|
| jun/2026 | Diagnóstico + plano | ✅ |
| jun/2026 | FormClientes → ClienteRepository (caso-modelo) | ✅ |
| jun/2026 | FormLogin → UsuarioRepository + SecurityHelper | ✅ |
| — | Próximas telas (ver checklist 1.1) | ⏳ |
