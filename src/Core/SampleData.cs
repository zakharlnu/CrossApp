using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Customer> Customers() =>
    [
        Customer.Create("C-001", "Олена Коваль", "olena@example.com"),
        Customer.Create("C-002", "Іван Петренко", phone: "+380501000002"),
        Customer.Create("C-003", "Марія Бондар", "maria@example.com"),
        Customer.Create("C-004", "Андрій Мельник", phone: "+380501000004"),
        Customer.Create("C-005", "Софія Шевченко", "sofia@example.com"),
        Customer.Create("C-006", "Максим Ткаченко", phone: "+380501000006"),
        Customer.Create("C-007", "Анна Кравченко", "anna@example.com"),
        Customer.Create("C-008", "Олександр Бойко", phone: "+380501000008"),
        Customer.Create("C-009", "Вікторія Ковальчук", "victoria@example.com"),
        Customer.Create("C-010", "Дмитро Олійник", phone: "+380501000010"),
        Customer.Create("C-011", "Катерина Поліщук", "kateryna@example.com"),
        Customer.Create("C-012", "Богдан Лисенко", phone: "+380501000012"),
        Customer.Create("C-013", "Наталія Романюк", "nataliia@example.com"),
        Customer.Create("C-014", "Артем Савчук", phone: "+380501000014"),
        Customer.Create("C-015", "Юлія Мороз", "yuliia@example.com"),
        Customer.Create("C-016", "Роман Гончар", phone: "+380501000016")
    ];

    public static IReadOnlyList<Order> Orders()
    {
        Order order1 = Order.Create("O-001", "C-001");
        order1.AddLine("P-001", "Ноутбук", 32999.99m, 1);
        order1.AddLine("P-002", "Бездротова миша", 849.50m, 1);
        order1.AddLine("P-010", "Підставка для ноутбука", 1499.50m, 1);

        Order order2 = Order.Create("O-002", "C-002");
        order2.AddLine("P-002", "Бездротова миша", 849.50m, 2);
        order2.Confirm();

        Order order3 = Order.Create("O-003", "C-003");
        order3.AddLine("P-003", "Механічна клавіатура", 2799.00m, 1);

        Order order4 = Order.Create("O-004", "C-004");
        order4.AddLine("P-004", "Монітор 27 дюймів", 10999.90m, 2);
        order4.AddLine("P-011", "Кабель HDMI", 499.00m, 2);
        order4.Confirm();

        Order order5 = Order.Create("O-005", "C-005");
        order5.AddLine("P-005", "USB-C адаптер", 1299.00m, 3);
        order5.Cancel();

        Order order6 = Order.Create("O-006", "C-006");
        order6.AddLine("P-006", "Зовнішній SSD 1 ТБ", 4199.75m, 1);
        order6.Confirm();

        Order order7 = Order.Create("O-007", "C-007");
        order7.AddLine("P-007", "Вебкамера", 2199.00m, 2);

        Order order8 = Order.Create("O-008", "C-008");
        order8.AddLine("P-008", "Гарнітура", 1899.95m, 1);
        order8.AddLine("P-007", "Вебкамера", 2199.00m, 1);
        order8.Confirm();

        Order order9 = Order.Create("O-009", "C-009");
        order9.AddLine("P-009", "Килимок для миші", 399.00m, 3);

        Order order10 = Order.Create("O-010", "C-010");
        order10.AddLine("P-010", "Підставка для ноутбука", 1499.50m, 1);
        order10.Confirm();
        order10.Cancel();

        Order order11 = Order.Create("O-011", "C-011");
        order11.AddLine("P-011", "Кабель HDMI", 499.00m, 2);

        Order order12 = Order.Create("O-012", "C-012");
        order12.AddLine("P-012", "Портативна колонка", 2499.00m, 1);
        order12.AddLine("P-015", "Павербанк", 1799.00m, 2);
        order12.AddLine("P-005", "USB-C адаптер", 1299.00m, 1);
        order12.Confirm();

        Order order13 = Order.Create("O-013", "C-013");
        order13.AddLine("P-013", "Принтер", 8999.00m, 1);

        Order order14 = Order.Create("O-014", "C-014");
        order14.AddLine("P-014", "Wi-Fi роутер", 2399.00m, 2);
        order14.Confirm();

        Order order15 = Order.Create("O-015", "C-015");
        order15.AddLine("P-015", "Павербанк", 1799.00m, 3);
        order15.AddLine("P-005", "USB-C адаптер", 1299.00m, 2);
        order15.Cancel();

        return
        [
            order1,
            order2,
            order3,
            order4,
            order5,
            order6,
            order7,
            order8,
            order9,
            order10,
            order11,
            order12,
            order13,
            order14,
            order15
        ];
    }
}
