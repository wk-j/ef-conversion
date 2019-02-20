#! "netcoreapp2.2"
#r "nuget:Microsoft.EntityFrameworkCore,2.2.2"

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

void A() {
    var converter = new BoolToStringConverter("OFF", "ON");
    var expr = converter.ConvertFromProviderExpression.Compile();

    var off = expr("OFF");
    var on = expr("ON");

    Console.WriteLine("2s OFF {0}", off);
    Console.WriteLine("2s ON {0}", on);
}

void B() {
    var converter = new BoolToTwoValuesConverter<string>("OFF", "ON");
    var expr = converter.ConvertFromProviderExpression.Compile();

    var off = expr("OFF");
    var on = expr("ON");

    Console.WriteLine("2v OFF {0}", off);
    Console.WriteLine("2v ON {0}", on);
}

A();
B();
