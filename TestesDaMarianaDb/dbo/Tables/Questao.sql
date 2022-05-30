CREATE TABLE [dbo].[Questao] (
    [Numero]        INT          IDENTITY (1, 1) NOT NULL,
    [Enunciado]     VARCHAR (50) NOT NULL,
    [Id_Disciplina] INT          NOT NULL,
    [Id_Materia]    INT          NOT NULL,
    CONSTRAINT [PK_Questao] PRIMARY KEY CLUSTERED ([Numero] ASC),
    CONSTRAINT [FK_Id_Disciplina] FOREIGN KEY ([Id_Disciplina]) REFERENCES [dbo].[Disciplina] ([Numero]),
    CONSTRAINT [FK_Id_Materia] FOREIGN KEY ([Id_Materia]) REFERENCES [dbo].[Materia] ([Numero])
);

