#:include quest.cs
#:property NoWarn=CA2266

const int Part = 1;

var ec = new EverybodyCodesClient();
var input = await ec.GetInputAsync(Quest.Number, Part);

Console.WriteLine(input);

//await ec.SubmitAsync(Quest.Number, Part, answer);
