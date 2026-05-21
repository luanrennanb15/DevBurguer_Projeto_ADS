# 🔥 DevBurguer - Aplicação Web Refatorada

Versão **profissional e escalável** da aplicação web DevBurguer, desenvolvida com **arquitetura modular**, separação de responsabilidades e padrões de desenvolvimento web modernos.

## 📋 Índice

- [Características](#características)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Como Usar](#como-usar)
- [Configuração](#configuração)
- [Estrutura de Arquivos](#estrutura-de-arquivos)
- [Guia de Desenvolvimento](#guia-de-desenvolvimento)
- [Troubleshooting](#troubleshooting)

---

## ✨ Características

### Frontend
✅ **Design Responsivo** - Mobile-first, otimizado para todos os tamanhos
✅ **Performance** - Sem dependências externas, carregamento rápido
✅ **UX/UI Moderno** - Interface baseada na logo épica do DevBurguer
✅ **Acessibilidade** - Código semântico e navegação por teclado
✅ **PWA Ready** - Pode funcionar offline com service workers (futuro)

### Funcionalidades
✅ **Catálogo Dinâmico** - 16 produtos em 5 categorias
✅ **Carrinho Completo** - Adicionar, remover, alterar quantidades
✅ **Checkout Robusto** - Dados de cliente, entrega, pagamento
✅ **Integração WhatsApp** - Envio automático de pedidos
✅ **Histórico de Pedidos** - Salvos em localStorage
✅ **Promoções** - Seção dedicada a combos e ofertas
✅ **Top 3** - Produtos mais pedidos em destaque

### Código
✅ **Modular** - Separação em múltiplos arquivos
✅ **Sem Frameworks** - Vanilla JavaScript puro
✅ **Organizado** - Config, Data, Cart, UI, Checkout, Main
✅ **Documentado** - Comentários e JSDoc em funções
✅ **Escalável** - Fácil adicionar features

---

## 📁 Estrutura do Projeto

```
devburger-refactored/
│
├── index.html              # Arquivo principal HTML
│
├── css/
│   ├── reset.css          # Reset de estilos
│   ├── variables.css      # Variáveis CSS (cores, fonts, espaçamento)
│   ├── globals.css        # Estilos globais
│   ├── components.css     # Componentes reutilizáveis
│   ├── layouts.css        # Layouts específicos
│   └── responsive.css     # Media queries
│
├── js/
│   ├── config.js          # Configurações e seletores
│   ├── data.js            # Base de dados (produtos)
│   ├── cart.js            # Lógica do carrinho (classe)
│   ├── ui.js              # Funções de interface
│   ├── checkout.js        # Lógica de checkout
│   └── main.js            # Inicialização e listeners
│
└── README.md              # Este arquivo
```

---

## 🚀 Como Usar

### 1. Abrir a Aplicação
```bash
# Simplesmente abra no navegador
# Duplo clique em: index.html
# Ou arraste para o navegador
```

### 2. Configurar Número WhatsApp
**Arquivo:** `js/config.js` (linha ~24)

```javascript
const CONFIG = {
    lanchonete: {
        whatsapp: '5511977097728',
    },
};
```

**Formato:** `55` + DDD + 9 dígitos  
**Exemplo:** `5511987654321`

### 3. Testar
1. Adicione produtos ao carrinho
2. Clique no ícone 🛒
3. Clique em "Finalizar Pedido"
4. Preencha dados fictícios
5. Clique "Confirmar Pedido e Enviar para WhatsApp"
6. WhatsApp deve abrir automaticamente

✅ **Pronto!** A aplicação está funcionando.

---

## ⚙️ Configuração

### Alterar Dados da Lanchonete
**Arquivo:** `js/config.js`

```javascript
const CONFIG = {
    lanchonete: {
        nome: 'DevBurguer',
        telefone: '(15) 3333-3333',
        endereco: 'Rua Principal, 123 - Centro',
        whatsapp: '5515999999999',
    },
    // ...
};
```

### Alterar Taxa de Entrega
**Arquivo:** `js/config.js`

```javascript
entrega: {
    taxa: 6.00, // ← Altere para seu valor
    tagline: 'Sua tagline aqui',
},
```

### Adicionar Novo Produto
**Arquivo:** `js/data.js`

```javascript
const PRODUTOS = [
    // ... produtos existentes ...
    {
        id: 17,
        nome: 'Seu Novo Burger',
        preco: 25.90,
        categoria: 'burgers',
        descricao: 'Descrição do seu burger',
        emoji: '🍔',
        destaque: false,
    },
];
```

### Alterar Cores
**Arquivo:** `css/variables.css` (linhas 8-12)

```css
:root {
    --color-primary: #FF3A44;       /* Vermelho */
    --color-primary-dark: #E62E35;  /* Vermelho escuro */
    --color-primary-light: #FF5A65; /* Vermelho claro */
    
    --color-secondary: #00BCD4;     /* Cyan */
    --color-secondary-dark: #0097A7; /* Cyan escuro */
    --color-secondary-light: #4DD0E1; /* Cyan claro */
    
    /* ... outras variáveis ... */
}
```

---

## 📦 Estrutura de Arquivos

### `index.html`
O arquivo principal que estrutura todo o HTML semanticamente.

**Seções:**
- Header com navegação
- Hero section com CTA
- Seção de promoções
- Top 3 mais pedidos
- Cardápio com categorias
- Carrinho lateral
- Modal de checkout

**Atributos importantes:**
- `id` - Identifica elementos principais
- `data-*` - Metadados (futuro)
- `aria-*` - Acessibilidade
- Formulários com `name` e `id` únicos

### `css/variables.css`
Define todas as variáveis de design:
- **Cores:** primária, secundária, status
- **Tipografia:** fontes, tamanhos, pesos
- **Espaçamento:** padding, margin
- **Sombras, raios, transições**

💡 **Dica:** Altere apenas aqui para mudar todo o design!

### `css/reset.css`
Remove estilos padrão do navegador:
- Margens e paddings padrão
- Scroll suave
- Normaliza inputs, buttons, etc.

### `css/globals.css`
Estilos aplicados globalmente:
- Body, tipografia
- Links, imagens
- Formulários
- Utilities (visually-hidden, opacity, etc.)

### `css/components.css`
Componentes reutilizáveis:
- `.btn` - Botões (primário, secundário, outline)
- `.card` - Cards de produtos
- `.badge` - Badges/tags
- `.form-*` - Elementos de formulário
- `.modal` - Modals e overlays
- Animações (@keyframes)

### `css/layouts.css`
Layouts específicos:
- Header e navegação
- Hero section
- Seções de promoção e cardápio
- Carrinho lateral
- Modal de checkout
- Grids

### `css/responsive.css`
Media queries para diferentes tamanhos:
- Tablets (768px)
- Mobile (576px)
- XSmall (360px)
- Landscape orientation
- Print
- Reduced motion

### `js/config.js`
Configurações globais:
- Informações da lanchonete
- Número WhatsApp
- Categorias e pagamentos
- **Cache de seletores DOM** (otimização importante)
- Estado da aplicação

### `js/data.js`
Base de dados:
- Array `PRODUTOS` com 16 itens
- Array `PROMOCOES` com 3 itens
- Funções helper:
  - `getProdutoById(id)`
  - `getProdutosByCategoria(cat)`
  - `getProdutosDestaque(ids)`

### `js/cart.js`
Classe `Carrinho` com métodos:
- `adicionar(id)` - Adiciona produto
- `remover(indice)` - Remove item
- `alterarQuantidade(indice, qtd)`
- `getSubtotal()`, `getTotal()`
- `salvarNoLocal()`, `carregarDoLocal()`
- `gerarResumo(dados)` - Para WhatsApp

### `js/ui.js`
Funções de interface:
- `renderizarCategorias()`
- `renderizarProdutos(cat)`
- `renderizarPromocoes()`
- `renderizarTopProdutos()`
- `abrirCarrinho()`, `fecharCarrinho()`
- `abrirCheckout()`, `fecharCheckout()`
- `mostrarToast(msg, tipo)`
- Validações

### `js/checkout.js`
Lógica de checkout:
- `finalizarPedido(event)` - Processa pedido
- `salvarPedidoNoHistorico()`
- `recuperarHistoricoPedidos()`
- `gerarRelatorioPedidos()`
- `exportarPedidosCSV()`
- Funções de validação

### `js/main.js`
Inicialização:
- `inicializarApp()` - Setup inicial
- `configurarEventListeners()` - Registra listeners
- Event handlers para:
  - Carrinho (abrir/fechar)
  - Formulário (submit, change)
  - Teclado (Escape, Enter)
  - Navegação
- Setup produção/desenvolvimento

---

## 🛠️ Guia de Desenvolvimento

### Adicionar Nova Categoria

**1. Edite `js/config.js`:**
```javascript
categorias: [
    // ... categorias existentes ...
    { id: 'nova-cat', label: 'Nova Categoria', icon: '🎯' },
],
```

**2. Adicione produtos com essa categoria em `js/data.js`:**
```javascript
{
    id: 99,
    nome: 'Produto Novo',
    preco: 19.90,
    categoria: 'nova-cat', // ← Mesma categoria
    descricao: '...',
    emoji: '🎯',
},
```

**3. Pronto!** A categoria aparecerá automaticamente.

### Adicionar Nova Forma de Pagamento

**1. Edite `js/config.js`:**
```javascript
pagamentos: [
    // ... pagamentos existentes ...
    { id: 'boleto', label: 'Boleto', icon: '📋' },
],
```

**2. Atualize o HTML `index.html` se necessário:**
```html
<div class="payment-option">
    <input type="radio" id="boletoPayment" name="payment" value="boleto">
    <label for="boletoPayment" class="payment-label">Boleto 📋</label>
</div>
```

### Implementar Desconto

**Em `js/cart.js`, adicione método à classe Carrinho:**
```javascript
aplicarCodigo(codigo) {
    const descontos = {
        'DESC10': 0.10,  // 10% de desconto
        'DESC20': 0.20,  // 20% de desconto
    };
    
    if (descontos[codigo]) {
        this.desconto = descontos[codigo];
        this.atualizar();
        return true;
    }
    return false;
}
```

### Implementar Frete Dinâmico

**Em `js/checkout.js`:**
```javascript
function calcularFrete(cep) {
    // Integrar com API de cálculo de frete
    // Ex: Correios, Loggi, etc.
}
```

### Adicionar Service Worker (Offline)

**Crie `sw.js`:**
```javascript
self.addEventListener('install', e => {
    // Cache recursos
});

self.addEventListener('fetch', e => {
    // Serve do cache se offline
});
```

**Em `js/main.js`:**
```javascript
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('sw.js');
}
```

---

## 🐛 Troubleshooting

### ❌ Página em branco
1. Verifique console (F12 → Console)
2. Verifique se todos os arquivos CSS/JS estão no mesmo diretório
3. Tente hard refresh (Ctrl+Shift+R)

### ❌ WhatsApp não abre
1. Número está correto? (formato: 55 + DDD + 9 dígitos)
2. Tem WhatsApp instalado?
3. Teste manualmente: `https://wa.me/5515999999999?text=teste`

### ❌ Produtos não aparecem
1. Verifique `js/data.js` - há vírgula entre items?
2. Verifique console por erro de sintaxe
3. Valide JSON em: https://jsonlint.com/

### ❌ Carrinho não funciona
1. localStorage está habilitado?
2. Não está em incógnito?
3. Permissões do navegador?

### ❌ Estilos não aparecem
1. Verifique caminho dos arquivos CSS
2. Limpe cache (Ctrl+Shift+Delete)
3. Verifique se as variáveis CSS estão definidas

---

## 📊 Performance

### Otimizações implementadas:
✅ Sem frameworks - Zero dependências  
✅ Cache de seletores - Não faz querySelector a cada evento  
✅ Event delegation - Um listener para múltiplos elementos  
✅ localStorage - Persiste dados sem servidor  
✅ CSS Variables - Fácil customização sem recompile  
✅ Lazy loading - Imagens carregam sob demanda (futuro)  

### Métricas esperadas:
- **FCP (First Contentful Paint):** < 1s
- **LCP (Largest Contentful Paint):** < 2.5s
- **CLS (Cumulative Layout Shift):** < 0.1
- **Tamanho total:** ~150 KB (HTML + CSS + JS)

---

## 🔐 Segurança

### Implementações:
✅ XSS Prevention - Template literals com sanitização  
✅ CSRF Prevention - FormData validation  
✅ Data Storage - LocalStorage apenas (sem cookies sensíveis)  
✅ Input Validation - Validação de forms  
✅ Escape HTML - Se necessário, implementar DOMPurify  

### Recomendações:
- Implementar HTTPS em produção
- Validar dados no servidor (backend)
- Usar webhooks para confirmar pedidos
- Nunca armazenar dados sensíveis no localStorage

---

## 📱 Responsividade

Teste em diferentes tamanhos:
- **Desktop:** 1920x1080
- **Tablet:** 768x1024
- **Mobile:** 375x667
- **Landscape:** 1024x600

Use DevTools (F12) → Toggle device toolbar

---

## 🚀 Próximos Passos

### Curto Prazo (1-2 semanas)
- [ ] Implementar mais categorias
- [ ] Adicionar filtro por preço
- [ ] Implementar busca
- [ ] Melhorar fotos/descrições

### Médio Prazo (1 mês)
- [ ] Criar painel de admin (localStorage)
- [ ] Integrar com Google Analytics
- [ ] Implementar avaliações
- [ ] Setup PWA completo

### Longo Prazo (3+ meses)
- [ ] Backend Node.js/Python
- [ ] Banco de dados MongoDB/PostgreSQL
- [ ] Sistema de autenticação
- [ ] Integração de pagamentos
- [ ] App mobile nativa

---

## 📄 Licença

Desenvolvido para **DevBurguer** - 2026

---

## 👨‍💻 Desenvolvimento

### Para contribuir:
1. Clone/fork este repositório
2. Crie uma branch (`git checkout -b feature/sua-feature`)
3. Commit suas mudanças (`git commit -m 'Adiciona feature'`)
4. Push para a branch (`git push origin feature/sua-feature`)
5. Abra um Pull Request

### Padrões de código:
- Use nomes descritivos em português
- Adicione comentários em funções complexas
- Mantenha CSS organizado por arquivo
- Prefira `const` sobre `let` e `var`
- Use `async/await` para promises

---

## 📞 Suporte

Para dúvidas ou problemas:
1. Verifique a seção **Troubleshooting**
2. Abra uma issue no GitHub
3. Entre em contato via WhatsApp

---

**Desenvolvido com ❤️ para DevBurguer**

🔥 Vamos crescer juntos! 🔥
