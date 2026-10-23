# ============================================================================
# DEVBURGUER API — imagem de container (Node.js + Express)
# ----------------------------------------------------------------------------
# Containeriza a API para rodar de forma idêntica em qualquer ambiente
# (máquina local, Render, ou qualquer provedor que aceite containers).
# ============================================================================

# Imagem base enxuta e com versão fixada (LTS)
FROM node:22-alpine

# Diretório de trabalho dentro do container
WORKDIR /app

# Copia primeiro os manifestos e instala só as dependências de produção
# (essa ordem aproveita o cache de camadas do Docker)
COPY package*.json ./
RUN npm ci --omit=dev

# Copia o restante do código-fonte
COPY . .

# A porta é injetada pelo provedor em nuvem; 3001 é o padrão local
ENV PORT=3001
EXPOSE 3001

# Usuário não-root (boa prática de segurança)
USER node

# Comando de inicialização
CMD ["node", "src/server.js"]
