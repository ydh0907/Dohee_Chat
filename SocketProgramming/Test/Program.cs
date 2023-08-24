namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Line 1");
            Console.Write("Line 2");

            // 현재 커서 위치 저장
            int currentCursorTop = Console.CursorTop;

            // 현재 라인을 지우기 위해 빈 칸으로 덮어씌움
            Console.SetCursorPosition(0, currentCursorTop);
            Console.Write(new string(' ', Console.WindowWidth));

            // 커서 위치를 원래 위치로 이동
            Console.SetCursorPosition(0, currentCursorTop);

            Console.WriteLine("Line 3");
            Console.WriteLine("Line 4");

            Console.ReadLine();
        }
    }
}