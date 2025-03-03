namespace DebaitMyFeed.Tests.Fixtures;

/// <summary>
/// A collection is created to share the network fixture between all tests.
/// This ensures that all other fixtures can use the same network by injecting the network fixture.
/// </summary>
[CollectionDefinition("DockerCollection")]
public class DockerCollection : ICollectionFixture<NetworkFixture>;