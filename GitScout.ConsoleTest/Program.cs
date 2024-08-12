// See https://aka.ms/new-console-template for more information
using GitScout.Settings.Tests;

Console.WriteLine("Hello, World!");

new SettingsTests().Should_not_leak_by_instance();

