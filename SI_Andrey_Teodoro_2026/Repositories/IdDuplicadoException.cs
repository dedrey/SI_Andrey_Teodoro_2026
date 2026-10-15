namespace SI_Andrey_Teodoro_2026.Repositories;

public class IdDuplicadoException : Exception
{
    public IdDuplicadoException() : base("Já existe um registro com este ID.") { }
}
