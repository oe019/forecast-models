using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Reflection;
using MoreLinq;
using Repo;
using Forcasting.Services;
using Abstraction;
using System.Linq.Expressions;

namespace TestLayer
{

    public static class Program
    {   
        public static class Func
        {
         public static   Func<WSDDATAProduct, string> StringFormatPreparer = (WSDDATAProduct wsdDataforProduct) => string.Format("{0},{1},{2},{3}", wsdDataforProduct.Product, wsdDataforProduct.TotalSale, wsdDataforProduct.WeeklyAverage,
                    wsdDataforProduct.WeeklyStDev, Environment.NewLine);
          public static  Action<string, StringBuilder> StringBuilderAppender = (string subject, StringBuilder stringBuilder) => stringBuilder.AppendLine(subject);
        }
        public static string CSVpath = "C:\\DEVELOPER\\guncel\\FortellMe\\FortellMe\\Uploaded\\interview_data_2MonthSalesData.csv";
      
        public static void Main(string[] args)
        {
            //Func<WSDDATAProduct, string> StringFormatPreparer = (WSDDATAProduct wsdDataforProduct) => string.Format("{0},{1},{2},{3}", wsdDataforProduct.Product, wsdDataforProduct.TotalSale, wsdDataforProduct.WeeklyAverage,
            //         wsdDataforProduct.WeeklyStDev, Environment.NewLine);

            
            Services s = new Services();
            s.Path = CSVpath;
            IDataSource asd = s.InputDataFactory.CreateDataSource();
            IProcessedData Ipd =  s.CreateProcessedData(asd);
            IWSDAnalizer WSDan = s.ModelService.ConstantModel.CreateWSDAnalizer(Ipd);
            IEnumerable<WSDDATAProduct> ResultList = WSDan.WSDListForProduct;
            
            ResultList.PrepareForWrite(Func.StringFormatPreparer).BuildTheStrings(Func.StringBuilderAppender).MaterializeBuildedString("C:\\DEVELOPER\\DENEME_2MonthSalesData.csv");
               //FINISH ... : 40 sn
            
        }
    }
}
