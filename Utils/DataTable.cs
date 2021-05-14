﻿using System.Data;

namespace Utils
{
    public class UtilsDataTable
    {

        public static void DataRowIsNull(DataRow dr, string columnName, object val)
        {
            if (val is null || string.IsNullOrEmpty(val.ToString()))
                return;

            dr[columnName] = val;
        }
    }
}
