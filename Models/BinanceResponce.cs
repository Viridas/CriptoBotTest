public class Adv
{
    public string Price { get; set; }
}

public class Data
{
    public Adv Adv { get; set; }
}

public class MyApiResponse
{
    public List<Data> Data { get; set; }
}