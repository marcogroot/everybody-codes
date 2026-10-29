#:include quest.cs
#:property NoWarn=CA2266

const int Part = 1;

var ec = new EverybodyCodesClient();
var input = await ec.GetInputAsync(Quest.Number, Part);
//var input = "3,11,12,16,24,25";
var nums = input.Split(',').Select(int.Parse).ToList();


var last = nums.Last();
List<int> squares = [];
var count = 1;
while ((count * count) < last)
{
	squares.Add(count * count);
	count++;
}
squares.Add(count * count);
//squares.ForEach(Console.WritqeLine);


var i = 0;
var sum = 0;
foreach (var num in nums)
{
	while (squares[i] < num)
	{
		i++;
	}

	sum += squares[i] - num;
}

await ec.SubmitAsync(Quest.Number, Part, sum);
