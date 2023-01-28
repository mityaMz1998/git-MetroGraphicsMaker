namespace WpfApplication1.LoadData.Reflection
{
    interface IDbTypeConverter
    {
        ADOX.DataTypeEnum Convert(System.Type type);

        System.Type Convert(ADOX.DataTypeEnum type);
    }
}
