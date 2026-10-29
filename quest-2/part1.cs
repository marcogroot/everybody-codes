#:include quest.cs
#:property NoWarn=CA2266
using System;
using System.Text;

const int Part = 1;

var ec = new EverybodyCodesClient();

var input = await ec.GetInputAsync(Quest.Number, Part)!;
//var input = "4<3 2<5 3>1 1>2 1<5 4<1 3>2 4<2 3>5 4<5\n4>1 2>1 3>2 1<5 4>5 2<5 3>1 4>3 2<4 5<3\n5>1 5>2 4>1 2<4 3>2 3<4 1>2 3>5 1<3 4>5";

Console.WriteLine(input);

var lines = input.Split('\n');


var answer = 0;
foreach (var line in lines)
{
	var counts = new Dictionary<char, int>();
	var nums = line.Split(' ');
	foreach (var num in nums)
	{
		var a = num[0];
		var c = num[2];
		counts[a] = 0;
		counts[c] = 0;
	}

	foreach (var num in nums)
	{
		var a = num[0];
		var c = num[1];
		var b = num[2];

		// Increment or set to 1
		if (c == '>')
		{
			counts[a] += 1;
			counts[b] -= 1;
		}
		else
		{
			counts[a] -= 1;
			counts[b] += 1;

		}
	}

	var sortedByValue = counts.OrderBy(pair => -pair.Value);
	StringBuilder sb = new StringBuilder("");

	foreach (var (name, age) in sortedByValue) // Direct deconstruction
	{
		sb.Append(name);
	}

	var abc = sb.ToString();
	answer += int.Parse(abc);
}

Console.WriteLine(answer);
await ec.SubmitAsync(Quest.Number, Part, answer);
