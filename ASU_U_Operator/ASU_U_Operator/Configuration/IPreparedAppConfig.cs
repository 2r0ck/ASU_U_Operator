namespace ASU_U_Operator.Configuration
{
    public interface IPreparedAppConfig
    {
        OperatorSection Operator { get; }

        bool Validate();
    }
}