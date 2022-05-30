CREATE TABLE [dbo].[Alternativa] (
    [Numero]     INT           IDENTITY (1, 1) NOT NULL,
    [Descricao]  VARCHAR (500) NOT NULL,
    [Correta]    BIT           NOT NULL,
    [Letra]      CHAR (1)      NOT NULL,
    [Id_Questao] INT           NOT NULL,
    CONSTRAINT [PK_Alternativa] PRIMARY KEY CLUSTERED ([Numero] ASC),
    CONSTRAINT [FK_Id_Questao] FOREIGN KEY ([Id_Questao]) REFERENCES [dbo].[Questao] ([Numero])
);

