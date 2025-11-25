using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Payload
{
    public class Payload
    {
        public byte[] Data { get;set; }
        public string Name { get;set; }
        public string LocalPath { get; set; }

        public Payload()
        {
            Name = GenerateName();
        }

        public void WriteToFileSystem()
        {
            File.WriteAllBytes(this.LocalPath, this.Data);
        }

        public static string GenerateName()
        {
            var animals = new List<string>
{
    "Dog", "Cat", "Horse", "Cow", "Sheep", "Pig", "Chicken", "Rabbit", "Elephant", "Lion",
    "Tiger", "Bear", "Wolf", "Fox", "Deer", "Giraffe", "Zebra", "Monkey", "Gorilla", "Kangaroo",
    "Koala", "Leopard", "Cheetah", "Panda", "Camel", "Hippopotamus", "Rhinoceros", "Crocodile", "Alligator", "Dolphin",
    "Whale", "Shark", "Octopus", "Seal", "Penguin", "Eagle", "Owl", "Falcon", "Parrot", "Swan",
    "Peacock", "Goose", "Duck", "Turkey", "Rooster", "Lizard", "Snake", "Turtle", "Frog", "Toad",
    "Crab", "Lobster", "Shrimp", "Bee", "Butterfly", "Ant", "Spider", "Fly", "Mosquito", "Dragonfly",
    "Snail", "Worm", "Mouse", "Rat", "Squirrel", "Hedgehog", "Bat", "Raccoon", "Otter", "Beaver",
    "Mole", "Donkey", "Mule", "Ox", "Goat", "Llama", "Reindeer", "Bison", "Buffalo", "Porcupine",
    "Chameleon", "Iguana", "Sealion", "Stingray", "Starfish", "Seahorse", "Pigeon", "Crow", "Magpie", "Robin",
    "Hawk", "Vulture", "Flamingo", "Pelican", "Dove", "Chimpanzee", "Baboon", "Meerkat", "Lynx", "Jaguar"
};

            var qualities = new List<string>
{
    "Honest", "Kind", "Brave", "Loyal", "Creative", "Patient", "Generous", "Optimistic", "Reliable", "Hardworking",
    "Thoughtful", "Empathetic", "Caring", "Adaptable", "Confident", "Respectful", "Trustworthy", "Ambitious", "Cheerful", "Calm",
    "Courageous", "Disciplined", "Friendly", "Helpful", "Humble", "Independent", "Inventive", "Joyful", "Modest", "Open-minded",
    "Passionate", "Polite", "Positive", "Proactive", "Resourceful", "Sincere", "Supportive", "Tolerant", "Understanding", "Witty",
    "Balanced", "Bright", "Charming", "Considerate", "Dependable", "Determined", "Energetic", "Fair", "Forgiving", "Gentle",
    "Grateful", "Honorable", "Imaginative", "Innovative", "Inspiring", "Just", "Loving", "Mature", "Motivated", "Observant",
    "Organized", "Perceptive", "Persistent", "Playful", "Practical", "Punctual", "Rational", "Realistic", "Reflective", "Reliable",
    "Responsible", "Selfless", "Sensible", "Smart", "Sociable", "Spontaneous", "Stable", "Strong", "Sympathetic", "Talented",
    "Trusting", "Upbeat", "Vibrant", "Warm", "Wise", "Zestful", "Clever", "Curious", "Faithful", "Focused",
    "Honorable", "Inventive", "Open-hearted", "Patient", "Perseverant", "Respectful", "Sincere", "Tactful", "Valiant", "Visionary"
};

            var random = new Random();

            string animal = animals[random.Next(animals.Count)];
            string quality = qualities[random.Next(qualities.Count)];

            string result = $"{quality}-{animal}";

            return result;
        }
    }
}
