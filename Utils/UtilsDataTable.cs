using System;
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

            if (dr.Table.Columns[columnName].DataType == typeof(DateTime))
            {
                DateTime dt = Convert.ToDateTime(val.ToString());
                if (dt.Year < 1753) {
                    val = dt.ToString("1753-MM-dd");
                }
            }
            dr[columnName] = val;
        }
    }
}
