using System;
using System.Collections.Generic;
using static SupportDeskLab.Utility;

namespace SupportDeskLab
{
    class Program
    {
        static int NextTicketId = 1;

        // Store customers (key = customer ID, value = Customer object)
        static Dictionary<string, Customer> Customers = new Dictionary<string, Customer>();

        // Store tickets in the order they were created
        static Queue<Ticket> Tickets = new Queue<Ticket>();

        // Stack to store undoable actions
        static Stack<UndoEvent> UndoEvents = new Stack<UndoEvent>();

        static void Main()
        {
            initCustomer();

            while (true)
            {
                Console.WriteLine("\n=== Support Desk ===");
                Console.WriteLine("[1] Add customer");
                Console.WriteLine("[2] Find customer");
                Console.WriteLine("[3] Create ticket");
                Console.WriteLine("[4] Serve next ticket");
                Console.WriteLine("[5] List customers");
                Console.WriteLine("[6] List tickets");
                Console.WriteLine("[7] Undo last action");
                Console.WriteLine("[0] Exit");
                Console.Write("Choose: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddCustomer(); break;
                    case "2": FindCustomer(); break;
                    case "3": CreateTicket(); break;
                    case "4": ServeNext(); break;
                    case "5": ListCustomers(); break;
                    case "6": ListTickets(); break;
                    case "7": Undo(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

  
        static void initCustomer()
        {
            Customers["C001"] = new Customer("C001", "Ava Martin", "ava@example.com");
            Customers["C002"] = new Customer("C002", "Ben Parker", "ben@example.com");
            Customers["C003"] = new Customer("C003", "Chloe Diaz", "chloe@example.com");
        }

        static void AddCustomer()
        {
            Console.Write("Enter Customer ID: ");
            string id = Console.ReadLine();

            if (Customers.ContainsKey(id))
            {
                Console.WriteLine("Customer ID already exists.");
                return;
            }

            Console.Write("Enter Name: ");
            string name = Console.ReadLine();
            Console.Write("Enter Email: ");
            string email = Console.ReadLine();

            Customer c = new Customer(id, name, email);
            Customers[id] = c;
            UndoEvents.Push(new UndoAddCustomer(c));

            Console.WriteLine($"Customer {name} added successfully!");
        }

        static void FindCustomer()
        {
            Console.Write("Enter Customer ID: ");
            string id = Console.ReadLine();

            if (Customers.TryGetValue(id, out Customer c))
            {
                Console.WriteLine("Customer found:");
                Console.WriteLine(c);
            }
            else
            {
                Console.WriteLine("Customer not found.");
            }
        }

        static void CreateTicket()
        {
            Console.Write("Enter Customer ID for the ticket: ");
            string id = Console.ReadLine();

            if (!Customers.ContainsKey(id))
            {
                Console.WriteLine("Customer ID not found.");
                return;
            }

            Console.Write("Enter ticket subject: ");
            string subject = Console.ReadLine();

            Ticket t = new Ticket(NextTicketId++, id, subject);
            Tickets.Enqueue(t);
            UndoEvents.Push(new UndoCreateTicket(t));

            Console.WriteLine($"Ticket #{t.TicketId} created for {Customers[id].Name}");
        }

        static void ServeNext()
        {
            if (Tickets.Count == 0)
            {
                Console.WriteLine("No tickets to serve.");
                return;
            }

            Ticket t = Tickets.Dequeue();
            UndoEvents.Push(new UndoServeTicket(t));

            Console.WriteLine($"Serving Ticket #{t.TicketId} for {Customers[t.CustomerId].Name}");
        }

        static void ListCustomers()
        {
            Console.WriteLine("-- Customers --");
            if (Customers.Count == 0)
            {
                Console.WriteLine("No customers available.");
                return;
            }

            foreach (var c in Customers.Values)
            {
                Console.WriteLine(c);
            }
        }

        static void ListTickets()
        {
            Console.WriteLine("-- Tickets (front to back) --");
            if (Tickets.Count == 0)
            {
                Console.WriteLine("No tickets in the queue.");
                return;
            }

            foreach (var t in Tickets)
            {
                Console.WriteLine(t);
            }
        }

        static void Undo()
        {
            if (UndoEvents.Count == 0)
            {
                Console.WriteLine("Nothing to undo.");
                return;
            }

            UndoEvent e = UndoEvents.Pop();

            if (e is UndoAddCustomer add)
            {
                if (Customers.Remove(add.Customer.CustomerId))
                    Console.WriteLine($"Undo → Removed customer {add.Customer.CustomerId}");
            }
            else if (e is UndoCreateTicket create)
            {
                Queue<Ticket> temp = new Queue<Ticket>();
                while (Tickets.Count > 0)
                {
                    Ticket t = Tickets.Dequeue();
                    if (t.TicketId != create.Ticket.TicketId)
                        temp.Enqueue(t);
                }
                while (temp.Count > 0)
                    Tickets.Enqueue(temp.Dequeue());

                Console.WriteLine($"Undo → Removed Ticket #{create.Ticket.TicketId}");
            }
            else if (e is UndoServeTicket serve)
            {
                Tickets.Enqueue(serve.Ticket);
                Console.WriteLine($"Undo → Re-added Ticket #{serve.Ticket.TicketId}");
            }
            else
            {
                Console.WriteLine("Unknown undo type.");
            }
        }
    }
}
