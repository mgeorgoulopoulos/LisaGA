using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lisa {
	class Evolver {

		const int populationSize = 50;
		const int rankSelection = 25;
		const double crossoverRate = 0.7;
		const double mutationRate = 0.001;

		const float positionMutationStep = 1.0f / 64.0f;
		const float colorMutationStep = 1.0f / 256.0f;


		public Specimen tick() {
			if (fitnessEvaluationQueue.Count <= 0) {
				updateGeneration();
			}

			return fitnessEvaluationQueue.Dequeue();
		}

		List<Specimen> population = new List<Specimen>();
		Queue<Specimen> fitnessEvaluationQueue = new Queue<Specimen>();
		Random rnd = new Random();

		public void initialize() { 
			// start with a random population
			for (int i = 0; i < populationSize; i++) {
				population.Add(Specimen.random);
			}

			// push all specimen for evaluation
			foreach (var s in population) {
				fitnessEvaluationQueue.Enqueue(s);
			}
		}

		int generationCount = 0;

		void updateGeneration() {
			generationCount++;

			// sort population by fitness
			population = population.OrderByDescending(o => o.fitness).ToList();

			printStats();

			// discard bottom-ranked specimen
			population.RemoveRange(rankSelection, populationSize - rankSelection);

			// produce offspring
			for (int i = 0; i < populationSize - rankSelection; i += 2) {
				produceOffspring();
			}
		}

		void printStats() { 
			double averageFitness = 0.0;
			foreach (var s in population) {
				averageFitness += s.fitness;
			}
			averageFitness /= (double)populationSize;
			double maxFitness = population[0].fitness;

			Console.Write("" + generationCount + "\t" + averageFitness + "\t" + maxFitness + "\n");
		}

		void produceOffspring() { 
			// pick two random parents from the top-ranking
			Specimen parent1 = topRankingRandom;
			Specimen parent2 = topRankingRandom;

			Specimen offspring1 = parent1.clone;
			Specimen offspring2 = parent2.clone;

			// crossover vertex chromosome
			if (performCrossover) {
				crossoverVerts(offspring1, parent1, parent2);
				crossoverVerts(offspring2, parent1, parent2);
			}

			// crossover color chromosome
			if (performCrossover) {
				crossoverColors(offspring1, parent1, parent2);
				crossoverColors(offspring2, parent1, parent2);
			}

			// mutate both offsprings
			mutate(offspring1);
			mutate(offspring2);

			// insert them to the population
			bool insertLast = false;
			population.Add(offspring1);
			if (population.Count < populationSize) population.Add(offspring2);
			else insertLast = true; // to maintain intended population size in spite of the offspring being produced in pairs

			// queue them for fitness evaluation
			fitnessEvaluationQueue.Enqueue(offspring1);
			if (!insertLast) fitnessEvaluationQueue.Enqueue(offspring2);
		}

		// crossover of vertex chromosome
		void crossoverVerts(Specimen offspring, Specimen parent1, Specimen parent2) { 
			// uniform crossover
			for (int i = 0; i < offspring.verts.Length; i++) { 
				if (rnd.NextDouble() < 0.5) {
					offspring.verts[i] = parent1.verts[i];
				} else {
					offspring.verts[i] = parent2.verts[i];
				}
			}
		}

		// crossover of color chromosome
		void crossoverColors(Specimen offspring, Specimen parent1, Specimen parent2) {
			// uniform crossover
			for (int i = 0; i < offspring.colors.Length; i++) {
				if (rnd.NextDouble() < 0.5) {
					offspring.colors[i] = parent1.colors[i];
				} else {
					offspring.colors[i] = parent2.colors[i];
				}
			}
		}

		// specimen mutation
		void mutate(Specimen s) {
			for (int i = 0; i < s.verts.Length; i++) {
				if (performMutation) s.verts[i].X = mutatePosition(s.verts[i].X);
				if (performMutation) s.verts[i].Y = mutatePosition(s.verts[i].Y);
				if (performMutation) s.verts[i].Z = mutatePosition(s.verts[i].Z);
			}

			for (int i = 0; i < s.colors.Length; i++) {
				if (performMutation) s.colors[i].R = mutateColor(s.colors[i].R);
				if (performMutation) s.colors[i].G = mutateColor(s.colors[i].G);
				if (performMutation) s.colors[i].B = mutateColor(s.colors[i].B);
			}
		}

		float mutatePosition(float x) {
			if (rnd.NextDouble() < 0.5) { 
				return x + (float) rnd.NextDouble() * (2.0f * positionMutationStep) - positionMutationStep;
			} else { 
				return (float)(rnd.NextDouble() * 2.0 - 1.0);
			}
		}

		float mutateColor(float x) {
			if (rnd.NextDouble() < 0.5) { 
			return Math.Min(1.0f, Math.Max(0.0f, x + (float)rnd.NextDouble() * (2.0f * colorMutationStep) - colorMutationStep));
			} else { 
				return (float)rnd.NextDouble();
			}
		}

		Specimen topRankingRandom {
			get { 
				return population[rnd.Next(rankSelection)];
			}
		}

		bool performCrossover {
			get {
				return rnd.NextDouble() < crossoverRate;
			}
		}

		bool performMutation {
			get {
				return rnd.NextDouble() < mutationRate;
			}
		}
	}
}
