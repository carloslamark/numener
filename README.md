# Numener 🧮

> **Aplicativo Gamificado para Rastreio de Indícios de Discalculia com Adaptação Dinâmica de Dificuldade via IA.**

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?style=flat&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-blue)
![AI](https://img.shields.io/badge/AI-Q--Learning-orange)
![Status](https://img.shields.io/badge/Status-Concluído-green)

## 📖 Sobre o Projeto

**Numener** é um *software* educacional desenvolvido como Trabalho de Conclusão de Curso (TCC) em Engenharia da Computação na **Universidade Federal do Vale do São Francisco (UNIVASF)**.

O objetivo do projeto é digitalizar o protocolo do teste padronizado *Numeracy Screener*, tornando o processo de triagem de dificuldades matemáticas (discalculia) mais engajador e menos ansiogênico para crianças. A aplicação utiliza técnicas de **Gamificação** e **Inteligência Artificial** para personalizar a experiência do usuário em tempo real.

---

## ✨ Funcionalidades Principais

### 🎮 Modos de Jogo
1.  **Modo História (Triagem):**
    * Segue rigorosamente os protocolos do *Numeracy Screener*.
    * Dividido em fases simbólicas (números) e não-simbólicas (pontos).
    * Coleta dados de tempo e precisão de forma silenciosa (*stealth assessment*).
2.  **Modo Infinito (Treino Adaptativo):**
    * Sistema de recompensa baseado em tempo ("Acerte para ganhar segundos").
    * Dificuldade ajustada dinamicamente pela IA.
    * Feedback visual imediato.

### 🤖 Inteligência Artificial (DDA)
O jogo implementa um agente de **Aprendizado por Reforço (Q-Learning)** para realizar o Ajuste Dinâmico de Dificuldade (DDA).
* **Técnica:** Q-Learning Tabular (*Model-Free*).
* **Estratégia:** Epsilon-Greedy (Exploração vs. Aproveitamento).
* **Otimização:** Utilização de *Reward Shaping* para evitar estagnação em níveis fáceis e incentivar o desafio pedagógico.
* **Treinamento:** O agente foi pré-treinado em ambiente simulado (Python) para evitar o problema de *Cold Start*.

### 💾 Persistência de Dados (Offline)
* Focado na realidade de escolas com infraestrutura restrita.
* Todos os dados são salvos localmente em arquivos **JSON**.
* Não requer conexão com a internet para funcionar ou gerar relatórios.

---

## 🛠️ Tecnologias Utilizadas

* **Engine:** Unity 2D.
* **Linguagem:** C#.
* **Simulação de IA:** Python (NumPy) para treinamento *offline* da Q-Table.
* **Design:** Figma (Prototipação) e *Assets* visuais autorais/licenciados.

---

## 📊 Resultados e Validação

O software foi submetido a uma validação técnica (*Expert Review*) com especialistas em ensino de matemática.

* **Usabilidade (SUS):** O projeto atingiu uma pontuação média de **94,0** na escala *System Usability Scale*, sendo classificado como **"Excelente"**.
* **Performance da IA:** Testes de estresse comprovaram que o algoritmo reage a erros do usuário em menos de 2 iterações, prevenindo a frustração cognitiva.

---

## 🚀 Como Executar

### Pré-requisitos
* [Unity Hub](https://unity.com/download) e Unity Editor (Versão recomendada: 2022.3 LTS ou superior).

### Passos
1.  Clone este repositório:
    ```bash
    git clone [https://github.com/carloslamark/numener.git](https://github.com/carloslamark/numener.git)
    ```
2.  Abra o Unity Hub e adicione a pasta do projeto clonado.
3.  Abra o projeto no Unity Editor.
4.  Abra a cena `MainMenu` (localizada em `Assets/Scenes`).
5.  Pressione **Play** ▶️.

> **Nota:** Para verificar os logs da IA ou os arquivos de save, acesse a pasta persistente do seu sistema (`AppData/LocalLow/Numener` no Windows).

---

## 👨‍💻 Autor

**Carlos Lamark de Barros Alencar**
* Graduando em Engenharia da Computação - UNIVASF
* [LinkedIn](https://www.linkedin.com/in/carlos-lamark/)

---

## 📄 Licença

Este projeto foi desenvolvido para fins acadêmicos. Sinta-se à vontade para estudar o código ou utilizá-lo como referência para projetos educacionais.
