#:include quest.cs
#:property NoWarn=CA2266

const int Part = 3;

var ec = new EverybodyCodesClient();
var input = await ec.GetInputAsync(Quest.Number, Part);

//var input = "3,11,12,16,24,25";
var nums = input.Split(',').Select(int.Parse).ToList();


var last = nums.Last();
List<int> walls = [];
var count = 3;
while ((count * count - 1) < last)
{
	walls.Add(count * count - 1);
	count += 2;
}
walls.Add(count * count - 1);
//walls.ForEach(Console.WriteLine);


var i = 0;
var sum = 0;
foreach (var num in nums)
{
	while (walls[i] < num)
	{
		i++;
	}

	//Console.WriteLine($"{walls[i]} {num}");
	sum += walls[i] - num;
}

Console.WriteLine(sum);

await ec.SubmitAsync(Quest.Number, Part, sum);
