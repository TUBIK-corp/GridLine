using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridLine_IDE.Helpers
{
    public static class SQLiteDataHelper
    {
        public static List<T> GetData<T>(this SQLiteCommand command) where T : new()
        {
            List<T> data = new List<T>();
            using(var reader = command.ExecuteReader())
            {
                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        T variable = new T();
                        var type = typeof(T);

                        var columns_count = reader.FieldCount;


                        try
                        {
                            foreach (var property in type.GetProperties())
                            {
                                if (property.CanWrite)
                                {
                                    for (int i = 0; i < columns_count; i++)
                                    {
                                        var name = reader.GetName(i);
                                        if (property.Name.ToLower().Equals(name.ToLower()))
                                            property.SetValue(variable, Convert.ChangeType(reader.GetValue(i), property.PropertyType));
                                    }
                                }
                            }
                            data.Add(variable);
                        }
                        catch
                        {
                            throw new Exception("Ошибка приведения. Сверьте свойства класса и колонки таблицы.");
                        }
                    }
                }
            }
            return data;
        }
    }
}
