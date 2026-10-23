# Etapa 9 — Gerenciamento Ágil do Projeto (PIM IV)

**Disciplina:** Gerenciamento de Projetos Ágil
**Projeto:** DevBurguer — Solução tecnológica integrada (Web, Mobile, API e Nuvem)
**Equipe:** 2 integrantes (continuação do grupo do PIM III)

Esta etapa apresenta o planejamento do desenvolvimento do DevBurguer com
metodologia ágil: papéis, Product Backlog, organização em sprints, cronograma,
quadro Kanban, definição das entregas e critérios de pronto.

---

## 9.1 Metodologia adotada

Adotou-se o **Scrum** como arcabouço de gestão, apoiado por um **quadro Kanban**
para visualizar o fluxo de trabalho. A escolha justifica-se pela natureza
incremental do projeto — a solução é entregue em partes funcionais sucessivas —
e pela necessidade de revisar o andamento a cada ciclo.

Como o grupo é pequeno (2 integrantes), os papéis do Scrum foram adaptados:

| Papel | Responsável | Atribuições |
|-------|-------------|-------------|
| Product Owner | Luan | Prioriza o backlog, define o valor de cada entrega |
| Scrum Master | Mauricio | Organiza as reuniões, remove impedimentos |
| Time de Desenvolvimento | Luan e Mauricio | Desenvolvem as funcionalidades |

**Cerimônias:** planejamento no início de cada sprint, reuniões rápidas de
acompanhamento (duas vezes por semana, dada a rotina de estudantes), revisão e
retrospectiva ao final de cada sprint.

**Duração das sprints:** 2 semanas.

---

## 9.2 Product Backlog

Lista priorizada de funcionalidades, escrita em formato de histórias de usuário.
Estimativa em *story points* (SP), escala de Fibonacci (1, 2, 3, 5, 8).
Prioridade: Alta, Média, Baixa.

| ID | História de usuário | Prioridade | SP |
|----|---------------------|:---------:|:--:|
| US01 | Como proprietário, quero um banco de dados único e íntegro para que todos os sistemas compartilhem os mesmos dados. | Alta | 8 |
| US02 | Como operador, quero registrar pedidos e acompanhar a produção para organizar o atendimento. | Alta | 8 |
| US03 | Como sistema, quero uma API REST para que site e aplicativo acessem os dados de forma segura. | Alta | 8 |
| US04 | Como operador, quero autenticação para que só pessoas autorizadas acessem a administração. | Alta | 5 |
| US05 | Como cliente, quero acessar o cardápio pelo site e fazer pedidos sem precisar ligar. | Alta | 8 |
| US06 | Como cliente, quero usar um aplicativo no celular para pedir de forma rápida. | Alta | 13 |
| US07 | Como cliente surdo, quero recursos de acessibilidade (Libras) para entender o cardápio. | Média | 3 |
| US08 | Como proprietário, quero relatórios e indicadores para tomar decisões com base em dados. | Média | 5 |
| US09 | Como equipe, quero a solução hospedada em nuvem com implantação automatizada para garantir disponibilidade. | Média | 8 |
| US10 | Como cliente, quero acompanhar o status do meu pedido em tempo real. | Baixa | 5 |

---

## 9.3 Organização em Sprints (Sprint Backlog)

| Sprint | Itens do backlog | Meta da sprint |
|--------|------------------|----------------|
| Sprint 1 | US01, US02 | Modelar e consolidar o banco de dados; operação interna base |
| Sprint 2 | US03, US04 | API REST com autenticação e segurança |
| Sprint 3 | US05, US08 | Site responsivo integrado à API; relatórios |
| Sprint 4 | US06, US10 | Aplicativo mobile integrado à API |
| Sprint 5 | US07, US09 | Acessibilidade, nuvem/DevOps e ajustes finais |

---

## 9.4 Cronograma das Sprints

| Sprint | Período (2026/2) | Entregável |
|--------|------------------|-----------|
| Sprint 0 — Planejamento | 11–22 ago | Backlog priorizado e definição do escopo |
| Sprint 1 | 25 ago – 05 set | Banco de dados completo (MER, procedures, triggers) |
| Sprint 2 | 08 – 19 set | API REST publicada com autenticação |
| Sprint 3 | 22 set – 03 out | Site responsivo em produção |
| Sprint 4 | 06 – 17 out | Aplicativo mobile funcional |
| Sprint 5 | 20 – 31 out | Infraestrutura em nuvem, acessibilidade e documentação |
| Entrega final | início nov | Documento do PIM IV e apresentação |

---

## 9.5 Quadro Kanban

Fluxo de trabalho em três colunas. Exemplo do estado ao final da Sprint 4:

| A Fazer (To Do) | Em Andamento (Doing) | Concluído (Done) |
|-----------------|----------------------|------------------|
| US07 — Acessibilidade em Libras | US06 — Aplicativo mobile | US01 — Banco de dados |
| US09 — Nuvem e DevOps | US10 — Status em tempo real | US02 — Pedidos e produção |
| — | — | US03 — API REST |
| — | — | US04 — Autenticação |
| — | — | US05 — Site responsivo |
| — | — | US08 — Relatórios |

As políticas do quadro: cada cartão representa uma história; um item só passa
para "Concluído" quando cumpre a Definição de Pronto (seção 9.7); limita-se o
trabalho em andamento para evitar tarefas acumuladas sem finalização.

---

## 9.6 Definição das Entregas (incrementos)

Cada sprint produz um incremento funcional, somando-se ao anterior:

1. **Incremento 1:** banco de dados estruturado e operação interna.
2. **Incremento 2:** API REST segura, conectando os sistemas.
3. **Incremento 3:** canal de vendas web integrado e relatórios gerenciais.
4. **Incremento 4:** aplicativo mobile consumindo a mesma API.
5. **Incremento 5:** solução hospedada em nuvem, acessível e documentada.

---

## 9.7 Definição de Pronto (Definition of Done)

Um item é considerado "Pronto" quando:

- a funcionalidade foi implementada e testada;
- está integrada aos demais módulos sem quebrar o que já funcionava;
- o código foi versionado no repositório Git do projeto;
- foi validada pelo time na revisão da sprint;
- a documentação correspondente foi atualizada.
