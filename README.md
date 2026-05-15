Pokemon Game
Aplicación móvil desarrollada en C# con .NET 8, construida en Visual Studio, que simula una experiencia de juego inspirada en la franquicia Pokemon. El proyecto permite al usuario interactuar con personajes, realizar combates y gestionar elementos propios del universo Pokemon a través de una interfaz móvil.
Tecnologías utilizadas: C#, .NET 8, Visual Studio.
Principios de diseño aplicados:
El proyecto aplica los principios SOLID como base de su arquitectura:

Principio de Responsabilidad Única (SRP): cada clase tiene una única razón para cambiar, separando la lógica de combate, la gestión de personajes y la interfaz de usuario en componentes independientes.
Principio Abierto/Cerrado (OCP): las clases están diseñadas para ser extendidas sin necesidad de modificar el código existente, permitiendo agregar nuevos tipos de Pokemon o habilidades sin alterar la lógica base.
Principio de Sustitución de Liskov (LSP): las clases derivadas pueden reemplazar a sus clases base sin afectar el comportamiento del sistema, aplicado en la jerarquía de Pokemon y sus tipos.
Principio de Segregación de Interfaces (ISP): las interfaces están definidas de forma específica para que las clases solo implementen los métodos que realmente necesitan.
Principio de Inversión de Dependencias (DIP): los módulos de alto nivel no dependen de los de bajo nivel, sino de abstracciones, facilitando la mantenibilidad y las pruebas del sistema.

Arquitectura del proyecto: el código está organizado siguiendo una separación clara entre modelos, servicios y presentación, lo que permite escalar el proyecto y mantener el código limpio y legible.
