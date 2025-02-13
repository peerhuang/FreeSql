﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.DynamicEntity;
using Xunit;

namespace FreeSql.Tests.DynamicEntity
{
    public class DynamicEntityTest
    {
        private static IFreeSql fsql = new FreeSqlBuilder().UseConnectionString(DataType.Sqlite,
                "data source=:memory:")
            .UseMonitorCommand(d => Console.WriteLine(d.CommandText)).Build();

        [Fact]
        public void NormalTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("NormalUsers")
                .Property("Id", typeof(string))
                .Property("Name", typeof(string))
                .Property("Address", typeof(string))
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Id"] = Guid.NewGuid().ToString(),
                ["Address"] = "北京市"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
        }

        [Fact]
        public void AttributeTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("AttributeUsers", new TableAttribute() { Name = "T_Attribute_User" },
                    new IndexAttribute("Name_Index1", "Name", false))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Address", typeof(string),
                    new ColumnAttribute() { StringLength = 150, Position = 3 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Address"] = "北京市"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            var insertId = fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteIdentity();
            var select = fsql.Select<object>().AsType(table.Type).ToList();
        }

        [Fact]
        public void SuperClassTest()
        {
            var table = fsql.CodeFirst.DynamicEntity("Roles", new TableAttribute() { Name = "T_Role" },
                    new IndexAttribute("Name_Index2", "Name", false))
                .Extend(typeof(BaseModel))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin"
            };
            var instance = table.CreateInstance(dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(table.Type);
            fsql.Insert<object>().AsType(table.Type).AppendData(instance).ExecuteAffrows();
        }
    }
    public class BaseModel
    {
        [Column(Position = 99)]
        public DateTime UpdateTime
        {
            get; set;
        }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson
        {
            get; set;
        }
    }
}