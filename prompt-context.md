1. Desafio
Bem-vindo ao desafio técnico da OniBus Express! Este desafio foi criado para avaliar suas habilidades práticas no desenvolvimento de software, cobrindo design de APIs

Trata-se de um projeto API que deve ser desenvolvido em .NET 8 (ASP.NET Core Web API), tendo como prinipios:
    - DDD
    - Solid
    - Padrão de pastas:
            src
              backend
                .api
                .application
                .domain
                .infra
              tests

Os nomes podem ser utilizados termos em inglês.

2. Contexto do Projeto
A OniBus Express é uma empresa de transporte rodoviário que precisa modernizar seu sistema de vendas. Você foi contratado para construir o MVP (Produto Mínimo Viável) do novo sistema, que deve permitir a busca e compra de passagens de ônibus online.

Entidades          Principais
Entidade:	       Campos
Rota:	           Origem, destino, duração estimada
Viagem:	           Rota associada, data/hora de partida, preço  base, assentos disponíveis
Passageiro:	       Nome, CPF, e-mail, data de nascimento
Reserva/Passagem:  Viagem, passageiro, número do assento, status, código de reserva

3. Requisitos do Backend (.NET)
Entregue esta parte se o foco da sua vaga é Back-End ou Full Stack.

3.1 Tecnologias Obrigatórias
.NET 8+ (ASP.NET Core Web API)
Entity Framework Core com banco relacional (PostgreSQL ou SQL Server)
Docker + docker-compose para subir o ambiente
Testes automatizados (xUnit)
Criação de autenticação com JWT

3.2 Funcionalidades Requeridas
Endpoints mínimos esperados:

GET    /rotas               — Listar todas as rotas disponíveis
GET    /viagens             — Buscar viagens por origem, destino e data
GET    /viagens/{id}        — Detalhes de uma viagem (assentos livres/ocupados)
POST   /reservas            — Criar reserva (nome, CPF, e-mail, viagem, assento)
GET    /reservas/{codigo}   — Consultar reserva pelo código gerado
DELETE /reservas/{codigo}   — Cancelar reserva

3.3 Regras de Negócio
Não deve ser possível reservar um assento já ocupado
Não deve ser possível reservar passagem para viagem já realizada
CPF deve ser validado (formato e dígito verificador)
O código de reserva deve ser único e legível (ex: ABC-12345)
Cancelamento só permitido até 2 horas antes da partida
3.4 Requisitos de Testes
Esperamos ao menos cobertura de testes unitários e/ou de integração para:

Validação do CPF
Regra de assento já ocupado
Regra de cancelamento dentro do prazo
Geração do código de reserva único
Dica: use um banco em memória (SQLite in-memory ou TestContainers) para os testes de integração. Não é necessário testar cada linha, mas mostre que você sabe onde os testes agregam valor.


4. Requisitos do Frontend (ReactJS)
Entregue esta parte se o foco da sua vaga é Front-End ou Full Stack.

4.1 Tecnologias Obrigatórias
React 18+ com TypeScript
Gerenciador de estado a sua escolha (Context API, Zustand, Redux, etc.)
Testes com React Testing Library + Jest ou Vitest
Docker para servir a aplicação (Nginx ou similar)

4.2 Telas Requeridas
Tela 1 — Busca de Passagens
Formulário com: Origem, Destino, Data de ida
Botão de buscar
Listagem de viagens disponíveis com preço, horário e vagas restantes
Estado de loading e mensagem quando não há resultados
Tela 2 — Seleção de Assento
Mapa visual dos assentos (livre / ocupado / selecionado)
Exibir informações da viagem: rota, data, hora, preço
Botão para prosseguir com o assento selecionado
Tela 3 — Dados do Passageiro e Confirmação
Formulário: Nome completo, CPF, E-mail
Validação dos campos no frontend
Resumo da compra antes de confirmar
Tela de sucesso com código da reserva após confirmação
Tela 4 (Bonus) — Consulta de Reserva
Campo para digitar o código da reserva
Exibir detalhes ou opção de cancelamento


