using System.Data;

namespace Utils
{

    public class UtilsDataTable
    {

        /// <summary>
        /// 空值判断
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName">字段名</param>
        /// <param name="val">值</param>
        public static void DataRowIsNull(DataRow dr, string columnName, object val)
        {
            if (val is null || string.IsNullOrEmpty(val.ToString()))
                return;

            dr[columnName] = val;
        }
    }
}
