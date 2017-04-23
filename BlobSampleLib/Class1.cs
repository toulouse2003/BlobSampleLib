using ClassLibraryUtilBlob;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace BlobSampleLib {
    public class Employee {
        public static List<Employee> employees;
        public static MSSQLWrapper dbh;
        private int id;
        private string firstname;
        private string lastname;
        private string title;
        private DateTime hiredate;
        private int reportsto;
        private byte[] photo;

        public Employee(int id, string fn, string ln, string title, DateTime hd, int boss, string imgFile) {
            this.id = id;
            this.firstname = fn;
            this.lastname = ln;
            this.title = title;
            this.hiredate = hd;
            this.reportsto = boss;
            this.imageFile2ByteArray(imgFile);
        }

        public Employee(int id, string fn, string ln, string title, DateTime hd, int boss) {
            this.id = id;
            this.firstname = fn;
            this.lastname = ln;
            this.title = title;
            this.hiredate = hd;
            this.reportsto = boss;
        }
        public int getId() {
            return this.id;
        }
        public string getName() {
            return this.firstname + " " + this.lastname;
        }
        public string getTitle() {
            return this.title;
        }
        public DateTime getHiredate() {
            return this.hiredate;
        }
        public override string ToString() {
            string s = string.Format("{0}  {1}", this.firstname, this.lastname);
            return s;
        }

        public void emp2Db() {
            // insert into db
            dbh = MSSQLWrapper.getConnection();
            dbh.open();
            string sql = "";
            sql += "insert into employee (firstname, lastname, title, hiredate, reportsto, photo) ";
            sql += "values (@fn, @ln, @ti, @hi, @re, @photo)";
            try {
                dbh.setCommand(sql);

                SqlParameter fnP = new SqlParameter("@fn", SqlDbType.Text, 32);
                fnP.Value = this.firstname;
                dbh.addParam(fnP);
                SqlParameter lnP = new SqlParameter("@ln", SqlDbType.Text, 32);
                lnP.Value = this.lastname;
                dbh.addParam(lnP);
                SqlParameter tiP = new SqlParameter("@ti", SqlDbType.Text, 32);
                tiP.Value = this.title;
                dbh.addParam(tiP);
                SqlParameter hiP = new SqlParameter("@hi", SqlDbType.DateTime);
                hiP.Value = this.hiredate;
                dbh.addParam(hiP);
                SqlParameter reP = new SqlParameter("@re", SqlDbType.Int);
                reP.Value = this.reportsto;
                dbh.addParam(reP);

                SqlParameter phP = new SqlParameter("@Photo", SqlDbType.Image, photo.Length);
                phP.Value = this.photo;
                dbh.addParam(phP);

                dbh.prepare();
                dbh.otherQuery();
            } catch (Exception ex) {
                Console.WriteLine(sql);
                Console.WriteLine(this.reportsto);
                throw ex;
            }
            dbh.close();
        }
        public void imageFile2ByteArray(string filePath) {
            FileStream stream = new FileStream(
                filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);

            this.photo = reader.ReadBytes((int) stream.Length);

            reader.Close();
            stream.Close();
        }
        public byte[] getImageData() {
            dbh = MSSQLWrapper.getConnection();
            string sql = string.Format("SELECT photo FROM employee WHERE id = {0}", this.id);
            dbh.setCommand(sql);
            dbh.open();
            object value = dbh.getCommand().ExecuteScalar(); // returns row 1, col 1 from rs
            dbh.close();
            if (value != null) {
                return (byte[]) value;
            }
            return null;
        }

        public static Employee empFromList(int id) {
            // find from list
            foreach (Employee e in Employee.employees) {
                if (e.getId() == id)
                    return e;
            }
            return null;
        } 
        public static void emps2List() {
            dbh = MSSQLWrapper.getConnection();
            employees = new List<Employee>();

            string sql = "";
            sql += "select id, firstname, lastname, title, hiredate, reportsto";
            sql += " from employee";
            dbh.open();
            try {
                dbh.setCommand(sql);
                SqlDataReader res = dbh.select();
                while (res.Read()) {
                    Employee e = new Employee(Convert.ToInt32(res["id"])
                                            , res["firstname"].ToString()
                                            , res["lastname"].ToString()
                                            , res["title"].ToString()
                                            , Convert.ToDateTime(res["hiredate"])
                                            , Convert.ToInt32(res["reportsto"]));
                    employees.Add(e);
                }
            } catch (Exception ex) {
                throw ex;
            }
            dbh.close();
        }
    }
}
