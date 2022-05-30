CREATE TABLE [dbo].[Materia] (
    [Numero]        INT          IDENTITY (1, 1) NOT NULL,
    [Nome]          VARCHAR (50) NOT NULL,
    [Id_Disciplina] INT          NOT NULL,
    [Serie]         VARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Numero] ASC),
    FOREIGN KEY ([Id_Disciplina]) REFERENCES [dbo].[Disciplina] ([Numero])
);

