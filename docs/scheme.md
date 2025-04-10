# Generation snapshot scheme

```json
{
    "Generation": int,
    "Best": {
        "Id": int,
        "NodeGenes": {
            "<node_id>":  {
                "Id": int,
                "Type": 0 - input, 1 - hidden, 2 - output
            }
        },
        "ConnectionGenes": {
            "<connectoin_id>":  {
                "Id": int,
                "Connection": {
                    "Input": int,
                    "Output": int
                },
                "Weight": float,
                "Status": 0 - enabled, 1 - disabled
            }
        },
        "Fitness": float
    },
    "Species": {
        "<species_id>": {
            "Id": int,
            "LastImproved": 0,
            "Members": {
                "<genome_id>": {
                    "Id": int,
                    "NodeGenes": {
                        "<node_id>":  {
                            "Id": int,
                            "Type": 0 - input, 1 - hidden, 2 - output
                        }
                    },
                    "ConnectionGenes": {
                        "<connectoin_id>":  {
                            "Id": int,
                            "Connection": {
                                "Input": int,
                                "Output": int
                            },
                            "Weight": float,
                            "Status": 0 - enabled, 1 - disabled
                        }
                    },
                    "Fitness": float
                },
                "<genome_id>": {...}
            },
            "Fitness": float,
            "AdjustedFitness": float,
            "FitnessHistory"

        },
        "<species_id>": {...}
    },
    "config": {
        ...Unimportant...
    }
}
```