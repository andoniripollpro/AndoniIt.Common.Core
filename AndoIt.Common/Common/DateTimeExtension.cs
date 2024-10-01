using System;

namespace AndoIt.Common.Common
{
    public static class DateTimeExtension
    {
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static string Formated(this DateTime dateTime)
        {
            return $"{dateTime:yyyy-MM-dd HH:mm:ss ('GMT'z)}";
        }

        public static DateTime ObtenerFechaMayor(DateTime[] fechas)
        {
            if (fechas == null || fechas.Length == 0)
            {
                throw new ArgumentException("El array de fechas no puede estar vacío.");
            }

            DateTime fechaMayor = fechas[0];

            foreach (DateTime fecha in fechas)
            {
                if (fecha > fechaMayor)
                {
                    fechaMayor = fecha;
                }
            }

            return fechaMayor;
        }
    }
}
