using Bogus;

namespace Common;

public record EmailTask(string LAEStab, string Email, string Status)
{
    public static EmailTask Create(string laeStab, string email, string status)
    {
        return new  EmailTask(laeStab, email, status);
    }

    public static List<EmailTask> Collection(int amount, Faker? faker = null)
    {
        faker ??= new Faker { Random = new Randomizer(1234) };
        var responses = new List<EmailTask>();

        for (var i = 0; i < amount; i++)
        {
            responses.Add(
                new EmailTask(
                    faker.Random.ReplaceNumbers("#######"),
                    faker.Internet.Email(provider: "education.gov.uk"),
                    faker.Random.Number(1, 3) switch
                    {
                        1 => "No Data",
                        2 => "In Progress",
                        _ => "Complete",
                    }
                )
            );
        }

        return responses;
    }
}