using MyPetShop_v3.Models;
using Org.BouncyCastle.Cms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace MyPetShop_v3.Models
{
    public class DB
    {
        private static string ConnectionString = @"Data Source=Zhangzhen;Initial Catalog=MyPetShop;Integrated Security=True;MultipleActiveResultSets=True;Connect Timeout=30;Encrypt=False;";
        private static SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine("打开数据库连接时出错：" + ex.Message);
                return null;
            }
        }
        // 关闭数据库连接
        private static void CloseConnection(SqlConnection connection)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
            }
        }
        //登录注册
        static public bool Login(string phone, string password, string name)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM PetOwner WHERE Phone=@uPhone AND Password=@uPassword AND Name=@uName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@uPhone", phone);
                    command.Parameters.AddWithValue("@uPassword", password);
                    command.Parameters.AddWithValue("@uName", name);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public static bool RegisterLogin(string name, string phone, string password, string state, string city, string address)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // 创建 SQL 查询语句
                string query = "INSERT INTO PetOwner (Name, Phone, Password, State, City, Address) VALUES (@Name, @Phone, @Password, @State, @City, @Address)";

                // 使用 SQLCommand 对象执行查询
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // 添加参数以防止 SQL 注入攻击
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Phone", phone);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@State", state);
                    command.Parameters.AddWithValue("@City", city);
                    command.Parameters.AddWithValue("@Address", address);

                    try
                    {
                        connection.Open();

                        // 检查手机号是否已存在
                        string checkQuery = "SELECT COUNT(*) FROM PetOwner WHERE Phone = @Phone";
                        using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@Phone", phone);
                            int existingPhoneCount = (int)checkCommand.ExecuteScalar();
                            if (existingPhoneCount > 0)
                            {
                                // 如果手机号已存在，则返回 false
                                return false;
                            }
                        }

                        // 执行插入操作
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
        }
        public static User FindUser(string phone)
        {
            User user = null;
            using (SqlConnection connection = OpenConnection())
            {
                if (connection == null)
                {
                    CloseConnection(connection);
                    return null;
                }

                string query = "SELECT * FROM PetOwner WHERE Phone = @Phone";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Phone", phone);
                    try
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User(reader["Phone"].ToString(), reader["Password"].ToString(), reader["Name"].ToString(), reader["State"].ToString(), reader["City"].ToString(), reader["Address"].ToString(), Convert.ToBoolean(reader["VIP"]), Convert.ToDecimal(reader["Balance"]));
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return user;
        }
        static public List<Pet> GetPetsShow()
        {
            List<Pet> pets = new List<Pet>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                string query = "SELECT TOP 6 ID, Category,Name,Price,Image, Descn FROM Pet WHERE IsPurchased=0 ORDER BY ID DESC";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {

                        return pets;
                    }
                    while (reader.Read())
                    {
                        Pet pet = new Pet(
                    reader.GetInt32(0),
                     reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDecimal(3),
                    reader.GetString(4),
                    reader.GetString(5));
                        pets.Add(pet);
                    }
                }
            }
            return pets;
        }

        public static void Purchase(int PetID, string Phone, out int OK, out string ErrorMessage)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.Purchase", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PetID", PetID);
                    cmd.Parameters.AddWithValue("@Phone", Phone);

                    SqlParameter okParam = new SqlParameter("@OK", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(okParam);

                    SqlParameter errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 4000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(errorMessageParam);

                    conn.Open();
                    //处理宠物已经被购买
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000 && ex.Class == 16)
                        {
                            OK = 0;
                            ErrorMessage = ex.Message;
                            return;
                        }
                        else
                        {
                            throw; // 抛出其他类型的异常
                        }
                    }
                    OK = (bool)okParam.Value ? 1 : 0;
                    ErrorMessage = errorMessageParam.Value as string;
                }
            }
        }

        public static User Recharge(string phone, string money)
        {
            if (!decimal.TryParse(money, out decimal parsedMoney))
            {
                // 金额无效，返回空值
                return null;
            }

            User user = null;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("dbo.Recharge", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Money", parsedMoney);

                    conn.Open();
                    // 执行存储过程
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // 读取存储过程返回的结果
                        if (reader.Read())
                        {
                            user = FindUser(phone);
                        }
                    }
                }
            }

            return user;
        }


    }

}