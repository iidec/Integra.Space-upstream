namespace Integra.Space.UnitTests
{
    public static class PersistentMockConstants
    {
        public const string TEST_SERVER_NAME = "TestServer";
        public const string SA_LOGIN_NAME = "sa";
        public const string SA_LOGIN_PASSWORD = "unaContraseñaDePrueba123456:!#$%/)/(";
        public const string SYSREADER_SERVER_ROLE_NAME = "sysreader";
        public const string SYSADMIN_SERVER_ROLE_NAME = "sysadmin";
        public const string MASTER_DATABASE_NAME = "master";
        public const string DBO_USER_NAME = "dbo";
        public const string DBO_SCHEMA_NAME = "dbo";

        // El prefijo "input" sirve para que en las pruebas se sepa que es una fuente de entrada para un stream.
        // Ver AddStream de la clase PersistentMockExtensions 
        // operación: IsInputSource = source.SourceName.StartsWith("input", StringComparison.InvariantCultureIgnoreCase) ? true : false
        public const string INPUT_SOURCE_NAME = "InputSource";

        public const string OUTPUT_SOURCE_NAME = "OutPutSource";
        public const string TEST_STREAM_NAME = "StreamTest";
        public const string QUERY_TEST_STREAM = ""; // esta cadena debe coincidir con el dll de prueba.
        public static byte[] ASSEMBLY_TEST_STREAM = { }; // aqui se puede obtener los bytes de un dll de pruebas para probar Ej. caidas, reboot, etc.
        public const string TEST_STREAM_COLUMN_NAME = "serverId";
        public static System.Type TEST_STREAM_COLUMN_TYPE = typeof(string);

        // para un ambiente extendido
        public const string ADMIN_LOGIN_1_NAME = "adminLogin1";
        public const string ADMIN_LOGIN_2_NAME = "adminLogin2";
        public const string NORMAL_LOGIN_1_NAME = "normalLogin1";
        public const string NORMAL_LOGIN_2_NAME = "normalLogin2";
        public const string NORMAL_LOGIN_3_NAME = "normalLogin3";

        public const string ADMIN_LOGIN_1_PASSWORD = "1234";
        public const string ADMIN_LOGIN_2_PASSWORD = "1234";
        public const string NORMAL_LOGIN_1_PASSWORD = "1234";
        public const string NORMAL_LOGIN_2_PASSWORD = "1234";
        public const string NORMAL_LOGIN_3_PASSWORD = "1234";

        public const string TEST_DATABASE_NAME = "tests";

        public const string ADMIN_USER1_NAME = "adminUser1";
        public const string ADMIN_USER2_NAME = "adminUser2";
        public const string NORMAL_USER_1_NAME = "normalUser1";
        public const string NORMAL_USER_2_NAME = "normalUser2";
        public const string NORMAL_USER_3_NAME = "normalUser3";

        public const string ROLE_1_NAME = "role1";
        public const string ROLE_2_NAME = "role2";

        public const string TEST_SCHEMA_NAME = "test";
    }
}
