using System;
using System.Data;
using System.Data.SqlClient;

namespace DapperFactory
{
    public class DbBatch
    {

        /// <summary>
        /// (单表)批量添加
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public static void InsertSingleTable(IDbConnection connection, DataTable dt, string tableName, IDbTransaction transaction = null)
        {
            //使用示例:
            //string strSql = $"SELECT * FROM Tb_TaProject WHERE 1<>1";
            //DataTable dt = new DbHelperSQLP(Bp.LoginSQLConnStr).Query(strSql).Tables[0];
            //DataRow dr;
            //for (int i = 0; i < length; i++)
            //{
            //    dr = dt.NewRow();
            //    dr["ID"] = Guid.NewGuid().ToString();
            //    dr["RID"] = i;
            //    dr["ZoneName"] = i;
            //    dt.Rows.Add(dr);
            //}
            //string result = BatchOperate.InsertSingleTable(Bp.LoginSQLConnStr, dt, "Tb_TaProject");

            if (dt.Rows.Count == 0) { return; }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)connection,SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
            {
                //bulkCopy.BulkCopyTimeout = 30;
                bulkCopy.BatchSize = dt.Rows.Count;
                bulkCopy.DestinationTableName = tableName;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    bulkCopy.ColumnMappings.Add(dt.Columns[j].ColumnName, dt.Columns[j].ColumnName);
                }

                bulkCopy.WriteToServer(dt);
            }
        }


        /// <summary>
        /// (单表)批量添加
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dt"></param>
        /// <param name="tableName"></param>
        public static (bool Result, string Msg) InsertSingleTable(string connectionString, DataTable dt, string tableName)
        {
            //使用示例:
            //string strSql = $"SELECT * FROM Tb_TaProject WHERE 1<>1";
            //DataTable dt = new DbHelperSQLP(Bp.LoginSQLConnStr).Query(strSql).Tables[0];
            //DataRow dr;
            //for (int i = 0; i < length; i++)
            //{
            //    dr = dt.NewRow();
            //    dr["ID"] = Guid.NewGuid().ToString();
            //    dr["RID"] = i;
            //    dr["ZoneName"] = i;
            //    dt.Rows.Add(dr);
            //}
            //string result = BatchOperate.InsertSingleTable(Bp.LoginSQLConnStr, dt, "Tb_TaProject");

            if (dt.Rows.Count == 0) { return (false, "数据为空"); }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.BulkCopyTimeout = 60;
                        bulkCopy.BatchSize = dt.Rows.Count;
                        bulkCopy.DestinationTableName = tableName;
                        try
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                bulkCopy.ColumnMappings.Add(dt.Columns[j].ColumnName, dt.Columns[j].ColumnName);
                            }

                            bulkCopy.WriteToServer(dt);
                            transaction.Commit();
                            return (true, "");
                        }
                        catch (Exception ex)
                        {

                            transaction.Rollback();
                            return (false, ex.Message + ex.StackTrace);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// (单表)批量更新
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="dt"></param>
        /// <param name="dtColumn"></param>
        /// <param name="tableName"></param>
        /// <param name="updateField"></param>
        /// <param name="whereField"></param>
        /// <returns></returns>
        public static bool UpdateSingleTable(string connectionString, DataTable dt, string tableName, string key, string[] updateField, string[] whereField)
        {
            //使用示例:
            //string tableName = "Tb_TaProject";
            //string strSql = $"SELECT ParentID,ID,RID,ZoneName,TypeID FROM {tableName}";
            //DataTable dt = DbHelperSQL.Query(strSql).Tables[0];
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    dt.Rows[i]["RID"] = i + 10;
            //    dt.Rows[i]["ZoneName"] = "item.ZoneName" + i + 10;
            //    dt.Rows[i]["TypeID"] = i + 10;
            //    dt.Rows[i]["ParentID"] = i + 10;
            //}
            //string result = BatchOperate.UpdateSingleTable(ConnectionString, dt, tableName, "ID", new string[] {"TypeID", "ParentID" }, new string[] { "ID" });

            bool result = true;
            if (dt.Rows.Count == 0) { return result; }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand(string.Empty, connection))
                    {
                        try
                        {
                            string strSql = string.Empty, tempTableName = $"#{tableName}_{Guid.NewGuid().ToString("N")}";
                            string columnDataType = string.Empty, columnLength = string.Empty;

                            command.Transaction = transaction;
                            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);

                            //更新的字段的排序
                            strSql = $@"SELECT COLUMN_NAME Name,DATA_TYPE Type,CHARACTER_MAXIMUM_LENGTH Length,NUMERIC_PRECISION Precision,NUMERIC_SCALE Scale FROM INFORMATION_SCHEMA.COLUMNS
                                 WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME IN('{key}','{string.Join("', '", updateField)}') ORDER BY ORDINAL_POSITION";
                            DataSet dsCreateColumn = new DataSet();
                            command.CommandText = strSql;
                            sqlDataAdapter.Fill(dsCreateColumn, tableName);
                            strSql = string.Empty;
                            foreach (DataRow dr in dsCreateColumn.Tables[0].Rows)
                            {
                                columnLength = string.Empty;
                                columnDataType = dr["Type"].ToString();
                                if (columnDataType == "varchar" || columnDataType == "nvarchar")
                                {
                                    columnLength = $"({ dr["Length"]})";
                                }
                                else if (columnDataType == "decimal")
                                {
                                    columnLength = $"({ dr["Precision"]},{dr["Scale"]})";
                                }
                                strSql += $" {dr["Name"]} {columnDataType}{columnLength},";
                            }
                            strSql = $" CREATE TABLE {tempTableName}({strSql.TrimEnd(',')});";

                            command.CommandText = strSql;
                            command.ExecuteNonQuery();

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                            {
                                bulkCopy.BatchSize = dt.Rows.Count;
                                bulkCopy.BulkCopyTimeout = 5000;
                                bulkCopy.DestinationTableName = tempTableName;
                                string columnName = "";
                                for (int i = 0; i < dt.Columns.Count; i++)
                                {
                                    columnName = dt.Columns[i].ColumnName;
                                    if (columnName == key || Array.IndexOf(updateField, columnName) > -1)
                                    {
                                        bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                                    }
                                }
                                bulkCopy.WriteToServer(dt);
                            }

                            strSql = $"UPDATE {tableName} SET ";
                            foreach (var itemField in updateField)
                            {
                                strSql += $"{tableName}.{itemField}={tempTableName}.{itemField},";
                            }
                            strSql = strSql.TrimEnd(',');
                            strSql += $" FROM {tempTableName} WHERE 1=1 ";

                            foreach (var itemField in whereField)
                            {
                                strSql += $" AND {tableName}.{itemField}={tempTableName}.{itemField} ";
                            }
                            strSql += $"; DROP TABLE {tempTableName};";

                            command.CommandText = strSql;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            result = false;
                            transaction.Rollback();

                        }
                    }
                }
            }
            return result;
        }
    }
}