# CLAUDE.md — App do garçom (PIM, DevBurguer)

Trabalho de faculdade (ADS). App Android nativo, em **Java**, que complementa o sistema DevBurguer. O garçom registra pedidos por mesa e eles entram no sistema existente.

## Como trabalhar comigo (regras mais importantes)

- **Um passo pequeno por vez.** Uma tela ou uma funcionalidade por vez. Nunca construa o app inteiro de uma vez.
- **Antes de escrever código, diga em 2-3 linhas o que vai fazer e espere eu confirmar.**
- **Depois de cada passo, explique em português simples o que cada trecho faz**, porque eu preciso defender o código na banca. Se eu pedir, explique de novo mais devagar.
- **Não mexa em nada fora desta pasta do app.** Não altere API, banco nem o sistema desktop: isso é do Luan.
- **Não invente endpoints, campos ou regras.** Se faltar informação sobre a API, pare e me pergunte. Use dados falsos (mock) claramente marcados até a API real existir.
- **Sem bibliotecas novas sem avisar e justificar** (ex.: Retrofit). Diga o que é, por que precisa e se há alternativa mais simples.
- Faça commit só quando eu pedir, uma mensagem curta por passo que funciona.
- Aponte riscos e decisões ruins com franqueza, sem concordar por padrão.

## Restrições do trabalho

- Android Studio + **Java** (não Kotlin, não Compose, não Flutter).
- `minSdk 26`, projeto em caminho sem acento (`C:\dev\...`).
- Prazo curto (1 a 2 semanas): escopo mínimo, nada de offline, nada de arquitetura elaborada.
- Testado no emulador do Android Studio.

## Padrão do professor (seguir à risca)

Referência: repositório do professor, projeto `Calculadora` (Java).

- Telas são **Activities** (`AppCompatActivity`) com layout XML. **Sem Fragments, sem Compose, sem ViewBinding.** Navegação entre telas por `Intent`.
- Cada Activity tem dois métodos, chamados em sequência no `onCreate`:
  - `configuracao()`: liga as variáveis aos componentes com `findViewById`.
  - `eventos()`: registra os cliques.
- Cliques com `new View.OnClickListener()` anônimo (não use lambda).
- Prefixos de nome nas variáveis e ids: `edt` (EditText), `btn` (Button), `txv` (TextView). Nomes em português, camelCase (`btnEnviarPedido`, `txvTotal`).
- **Todo texto da interface fica em `res/values/strings.xml`**, inclusive mensagens de erro. Nada de texto fixo no layout ou no Java.
- Chaves abertas na linha de baixo (estilo do professor).
- Pacote `com.example.<nome>`. Cores e tema em `colors.xml` e `themes.xml`.
- Layout horizontal opcional em `layout-land/`.
- Cada tela documentada em um `README.md` do projeto: estrutura de pastas, papel de cada arquivo, fluxo do app.

## Telas (protótipo)

Login do garçom → Mesas → Cardápio → Pedido (carrinho) → Comanda da mesa.

- Cada mesa tem uma **comanda**; cada envio é uma nova rodada de itens, e dá para adicionar mais depois.
- **Pagamento e fechamento da comanda são feitos no caixa (desktop), nunca no app.**

## Identidade visual (igual ao site)

Fundo `#0f0f11` e `#1c1c21`, vermelho principal `#FF3A44`, dourado `#FFD700`, verde `#27ae60`, fonte Poppins, botões em formato de pílula.

## API (DevBurguer, hospedada no Render)

- Node + PostgreSQL (Supabase). Plano gratuito do Render: a primeira chamada pode levar 30 a 50 segundos. O app deve mostrar carregamento e tratar timeout.
- **Existem hoje:** `GET /api/produtos`, `GET /api/categorias`, `POST /api/pedidos` (público, sem autenticação), `GET /api/pedidos/:id/status`.
- **Ainda em desenvolvimento pelo Luan:** login do garçom, mesas, comandas e rodadas. **Não presuma como serão.** Antes de integrar, peça o contrato real (rotas, corpo e resposta).
- **Não usar `POST /api/pedidos` para o garçom**: é público e não valida adicionais.
- Status usam texto exato do banco (ex.: `'Em Producao'`, sem acento).

## Como organizar o acesso a dados

Mantenha simples: uma classe para chamar a API e classes de modelo (`Produto`, `Mesa`, `Comanda`). Dados falsos e reais ficam no mesmo lugar, para trocar depois sem refazer as telas.
