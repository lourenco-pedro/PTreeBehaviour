<p align="center"> 
<img src="https://media.giphy.com/media/o1VM2fxHKPJSfNPRhB/giphy.gif" style="max-height: 300px;">
</p>

# PTreeBehaviour
Uma Behaviour Tree simples para criação de IA para seus projetos Unity
 
## Antes de começar

Esta behaviour não está 100% concluída, mas está o suficiente para vocês trabalharem por cima. Criei este sistema e o editor em um outro projeto pessoal meu, mas resolvi compartilhar com vocês para que possam implementar nos seus projetos Unity como entender.

# Como funciona

## A behaviour

Toda behaviour em PTreeBehaviour é um scriptableobject que pode ser criado através do menu Create > PTreeBehaviour > new Behaviour. Em cada behaviour criada irá ter todos os nodes configurados por você para que o NPC possa se comportar de forma única.

## BehaviourComponent

Para que o objeto possa de fato agir de acordo com o que foi configurado na Behaviour, ele precisará ter o PTreeBehaviourComponent atribuído à ele. Lá, terá uma field para que você possa adicionar qual Behaviour ele irá se comportar.

## Nodes

### Root

Onde todo comportamento do NPC irá começar, ponto inicial de partida.

### Condição

Node responsável por definir qual caminho o comportamento deverá seguir. Geralmente é nele que é feita o controle de estados

### Sequência

Node responsável por executar mais de uma ação.

### Ação

A ação que o NPC irá fazer, a conclusão de todo processamento de seu comportamento. Geralmente é nele que as mecânicas são aplicadas, como Andar, Atacar, Seguir, etc...

