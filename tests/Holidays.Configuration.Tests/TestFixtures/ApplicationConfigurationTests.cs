﻿using System.Collections.Concurrent;
using NUnit.Framework;

namespace Holidays.Configuration.Tests.TestFixtures;

[TestFixture]
public class ApplicationConfigurationTests
{
    [Test]
    public void valid_config_should_be_read_correctly()
    {
        var applicationConfiguration = new ApplicationConfiguration(
            "valid-configuration.json",
            environmentJsonFileName: "valid-configuration.{ENVIRONMENT}.json",
            overrideWithEnvironmentVariables: false);

        var testSettings = applicationConfiguration.Get<TestSettings>();

        Assert.Multiple(() =>
        {
            Assert.That(testSettings.StringSetting, Is.EqualTo("Hello"));
            Assert.That(testSettings.IntSetting, Is.EqualTo(33));
            Assert.That(testSettings.BoolSetting, Is.True);
        });
    }

    [Test]
    public void reading_valid_config_concurrently_should_succeed()
    {
        var applicationConfiguration = new ApplicationConfiguration(
            "valid-configuration.json",
            environmentJsonFileName: default,
            overrideWithEnvironmentVariables: false);

        var stringValues = new ConcurrentBag<string>();
        var intValues = new ConcurrentBag<int>();
        var boolValues = new ConcurrentBag<bool>();

        Parallel.For(0, 20, _ =>
        {
            var testSettings = applicationConfiguration.Get<TestSettings>();

            stringValues.Add(testSettings.StringSetting);
            intValues.Add(testSettings.IntSetting);
            boolValues.Add(testSettings.BoolSetting);
        });

        Assert.Multiple(() =>
        {
            Assert.That(stringValues.All(v => v == "Hello"), Is.True);
            Assert.That(intValues.All(v => v == 33), Is.True);
            Assert.That(boolValues.All(v => v), Is.True);
        });
    }

    [TestCase("invalid-configuration-1.json")]
    [TestCase("invalid-configuration-2.json")]
    [TestCase("invalid-configuration-3.json")]
    [TestCase("invalid-configuration-4.json")]
    public void reading_invalid_configuration_should_throw_exception(string configFileName)
    {
        var applicationConfiguration = new ApplicationConfiguration(
            configFileName,
            environmentJsonFileName: default,
            overrideWithEnvironmentVariables: false);

        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = applicationConfiguration.Get<TestSettings>();
        });
    }

    [Test]
    public void reading_not_existing_configuration_file_should_throw_exception()
    {
        var applicationConfiguration = new ApplicationConfiguration(
            "not-existing-file.json",
            environmentJsonFileName: default,
            overrideWithEnvironmentVariables: false);

        Assert.Throws<FileNotFoundException>(() =>
        {
            _ = applicationConfiguration.Get<TestSettings>();
        });
    }

    [Test]
    public void setting_from_environment_variable_should_be_overridden()
    {
        string readStringSetting;

        try
        {
            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                "Overridden value",
                EnvironmentVariableTarget.Process);

            var applicationConfiguration = new ApplicationConfiguration(
                "valid-configuration.json",
                environmentJsonFileName: default,
                overrideWithEnvironmentVariables: true);

            var testSettings = applicationConfiguration.Get<TestSettings>();

            readStringSetting = testSettings.StringSetting;
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                default,
                EnvironmentVariableTarget.Process);
        }

        Assert.That(readStringSetting, Is.EqualTo("Overridden value"));
    }

    [Test]
    public void setting_from_environment_variable_should_not_be_overridden()
    {
        string readStringSetting;

        try
        {
            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                "Overridden value",
                EnvironmentVariableTarget.Process);

            var applicationConfiguration = new ApplicationConfiguration(
                "valid-configuration.json",
                environmentJsonFileName: default,
                overrideWithEnvironmentVariables: false);

            var testSettings = applicationConfiguration.Get<TestSettings>();

            readStringSetting = testSettings.StringSetting;
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                default,
                EnvironmentVariableTarget.Process);
        }

        Assert.That(readStringSetting, Is.EqualTo("Hello"));
    }

    [Test]
    public void setting_from_development_file_should_override_setting_value()
    {
        string readStringSetting;

        try
        {
            Environment.SetEnvironmentVariable(
                "ENVIRONMENT",
                "Development",
                EnvironmentVariableTarget.Process);

            var applicationConfiguration = new ApplicationConfiguration(
                "valid-configuration.json",
                "valid-configuration.{ENVIRONMENT}.json",
                overrideWithEnvironmentVariables: false);

            var testSettings = applicationConfiguration.Get<TestSettings>();

            readStringSetting = testSettings.StringSetting;
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "ENVIRONMENT",
                default,
                EnvironmentVariableTarget.Process);
        }

        Assert.That(readStringSetting, Is.EqualTo("Hello overridden"));
    }

    [Test]
    public void setting_from_environment_variable_should_win_against_environment_json_file()
    {
        string readStringSetting;

        try
        {
            Environment.SetEnvironmentVariable(
                "ENVIRONMENT",
                "Development",
                EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                "Hello from environment variable",
                EnvironmentVariableTarget.Process);

            var applicationConfiguration = new ApplicationConfiguration(
                "valid-configuration.json",
                "valid-configuration.{ENVIRONMENT}.json",
                overrideWithEnvironmentVariables: true);

            var testSettings = applicationConfiguration.Get<TestSettings>();

            readStringSetting = testSettings.StringSetting;
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "ENVIRONMENT",
                default,
                EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable(
                "TestSection__StringSetting",
                default,
                EnvironmentVariableTarget.Process);
        }

        Assert.That(readStringSetting, Is.EqualTo("Hello from environment variable"));
    }
}
