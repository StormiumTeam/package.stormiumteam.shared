namespace package.guerro.shared.modding
{
    public enum ModType
    {
        Package,
        Mod
    }

    public enum IntegrationType
    {
        External              = 1,
        Internal              = 2,
        Integrated            = 4,
        ExternalAndIntegrated = 5,
        InternalAndIntegrated = 6,
    }

    public struct SModInfoData
    {
        public string          DisplayName;
        public string          NameId;
        public ModType         Type;
        public IntegrationType Integration;
    }
}