def get_genome(i, n, test_name="test-name"):
    """
    Get the nth genome from the ith generation with a specific test name.

    Args:
        i: The generation number
        n: The index of the genome to retrieve (0-based across all species)
        test_name: The test name prefix in the filename (default: "test-name")

    Returns:
        The genome data as a dictionary, or None if not found
    """
    import os
    import json
    import glob

    # Find the file for the ith generation with the specified test name
    file_pattern = f"{test_name}_{i}.json"
    matching_files = glob.glob(file_pattern)

    if not matching_files:
        raise FileNotFoundError(f"No files found matching pattern {file_pattern}")

    # Load the JSON data
    with open(matching_files[0], "r") as f:
        data = json.load(f)

    # Collect all genomes across all species
    all_genomes = []
    for species_id, species_data in data["Species"].items():
        for genome_id, genome_data in species_data["Members"].items():
            all_genomes.append(genome_data)

    # Check if we have enough genomes
    if n >= len(all_genomes):
        return None

    # Return the nth genome
    return all_genomes[n]


def get_best_genome(i, test_name="test-name"):
    """
    Get the best genome from the ith generation with a specific test name.

    Args:
        i: The generation number
        test_name: The test name prefix in the filename (default: "test-name")

    Returns:
        The best genome data as a dictionary, or None if not found
    """
    import os
    import json
    import glob

    # Find the file for the ith generation with the specified test name
    file_pattern = f"{test_name}_{i}.json"
    matching_files = glob.glob(file_pattern)

    if not matching_files:
        raise FileNotFoundError(f"No files found matching pattern {file_pattern}")

    # Load the JSON data
    with open(matching_files[0], "r") as f:
        data = json.load(f)

    # Return the best genome if it exists
    if "Best" in data:
        return data["Best"]
    else:
        # Fallback to finding the best genome manually if "Best" field doesn't exist
        best_genome = None
        best_fitness = float("-inf")

        for species_id, species_data in data["Species"].items():
            for genome_id, genome_data in species_data["Members"].items():
                if genome_data["Fitness"] > best_fitness:
                    best_fitness = genome_data["Fitness"]
                    best_genome = genome_data

        return best_genome


# def visualize_genome(genome, title=None, figsize=(10, 8)):
#     """
#     Visualize a genome as a neural network graph.

#     Args:
#         genome: The genome dictionary containing NodeGenes and ConnectionGenes
#         title: Optional title for the plot
#         figsize: Figure size tuple (width, height)

#     Returns:
#         The matplotlib figure object
#     """
#     import networkx as nx
#     import matplotlib.pyplot as plt
#     import matplotlib.patches as mpatches
#     from matplotlib.colors import LinearSegmentedColormap
#     import numpy as np

#     # Create a figure and axis
#     fig, ax = plt.subplots(figsize=figsize)

#     # Create a directed graph
#     G = nx.DiGraph()

#     # Node type colors
#     node_colors = {
#         0: "#3498db",  # Input nodes - blue
#         1: "#95a5a6",  # Hidden nodes - gray
#         2: "#e74c3c",  # Output nodes - red
#     }

#     node_labels = {0: "Input", 1: "Hidden", 2: "Output"}

#     # Add nodes to the graph
#     node_color_map = []
#     node_types = {}

#     for node_id, node_data in genome["NodeGenes"].items():
#         G.add_node(node_data["Id"])
#         node_type = node_data["Type"]
#         node_types[node_data["Id"]] = node_type
#         node_color_map.append(node_colors[node_type])

#     # Add connections to the graph
#     edge_weights = []
#     edges = []

#     for conn_id, conn_data in genome["ConnectionGenes"].items():
#         if conn_data["Status"] == 0:  # Only add enabled connections
#             source = conn_data["Connection"]["Input"]
#             target = conn_data["Connection"]["Output"]
#             weight = conn_data["Weight"]
#             G.add_edge(source, target, weight=weight)
#             edges.append((source, target))
#             edge_weights.append(weight)

#     # Create custom colormap for edges (blue for negative, red for positive)
#     colors = [(0, 0, 1), (0.8, 0.8, 0.8), (1, 0, 0)]  # Blue, Light Gray, Red
#     cmap = LinearSegmentedColormap.from_list("weight_colormap", colors, N=256)

#     # Normalize edge weights for coloring
#     if edge_weights:
#         vmin = min(edge_weights)
#         vmax = max(edge_weights)
#     else:
#         vmin, vmax = -1, 1  # Default range if no edges exist

#     # Create positions
#     # Group nodes by type
#     input_nodes = [n for n, t in node_types.items() if t == 0]
#     hidden_nodes = [n for n, t in node_types.items() if t == 1]
#     output_nodes = [n for n, t in node_types.items() if t == 2]

#     # Create positions
#     pos = {}

#     # Position input nodes in the first layer
#     if input_nodes:
#         input_y_positions = np.linspace(0, 1, len(input_nodes))
#         for i, node in enumerate(input_nodes):
#             pos[node] = (0, input_y_positions[i])

#     # Position output nodes in the last layer
#     if output_nodes:
#         output_y_positions = np.linspace(0, 1, len(output_nodes))
#         for i, node in enumerate(output_nodes):
#             pos[node] = (1, output_y_positions[i])

#     # Position hidden nodes in the middle
#     if hidden_nodes:
#         hidden_y_positions = np.linspace(0, 1, len(hidden_nodes))
#         for i, node in enumerate(hidden_nodes):
#             pos[node] = (0.5, hidden_y_positions[i])

#     # Draw the network
#     nx.draw_networkx_nodes(
#         G, pos, node_size=500, node_color=node_color_map, alpha=0.8, ax=ax
#     )

#     # Draw edges with color based on weight
#     for i, edge in enumerate(edges):
#         nx.draw_networkx_edges(
#             G,
#             pos,
#             edgelist=[edge],
#             width=2.0,
#             alpha=0.7,
#             edge_color=[edge_weights[i]],
#             edge_cmap=cmap,
#             edge_vmin=vmin,
#             edge_vmax=vmax,
#             ax=ax,
#         )

#     # Add labels
#     nx.draw_networkx_labels(G, pos, font_size=10, font_weight="bold", ax=ax)

#     # Create legend for node types
#     legend_elements = [
#         mpatches.Patch(color=node_colors[0], label=node_labels[0]),
#         mpatches.Patch(color=node_colors[1], label=node_labels[1]),
#         mpatches.Patch(color=node_colors[2], label=node_labels[2]),
#     ]
#     ax.legend(handles=legend_elements, loc="upper right")

#     # Add a colorbar for edge weights
#     sm = plt.cm.ScalarMappable(cmap=cmap, norm=plt.Normalize(vmin=vmin, vmax=vmax))
#     sm.set_array([])
#     plt.colorbar(sm, ax=ax, label="Connection Weight")

#     # Add title if provided, otherwise use default
#     if title:
#         ax.set_title(title)
#     else:
#         ax.set_title(f"Genome Network (Fitness: {genome.get('Fitness', 'N/A')})")

#     # Remove axis ticks and labels
#     ax.set_xticks([])
#     ax.set_yticks([])
#     ax.set_frame_on(False)

#     plt.tight_layout()

#     plt.show()


def visualize_genome(genome, title=None, figsize=(10, 8)):
    """
    Visualize a genome as a neural network graph with better distribution of hidden neurons.

    Args:
        genome: The genome dictionary containing NodeGenes and ConnectionGenes
        title: Optional title for the plot
        figsize: Figure size tuple (width, height)

    Returns:
        The matplotlib figure object
    """
    import networkx as nx
    import matplotlib.pyplot as plt
    import matplotlib.patches as mpatches
    from matplotlib.colors import LinearSegmentedColormap
    import numpy as np

    # Create a figure and axis
    fig, ax = plt.subplots(figsize=figsize)

    # Create a directed graph
    G = nx.DiGraph()

    # Node type colors
    node_colors = {
        0: "#3498db",  # Input nodes - blue
        1: "#95a5a6",  # Hidden nodes - gray
        2: "#e74c3c",  # Output nodes - red
    }

    node_labels = {0: "Input", 1: "Hidden", 2: "Output"}

    # Add nodes to the graph
    node_color_map = []
    node_types = {}

    for node_id, node_data in genome["NodeGenes"].items():
        G.add_node(node_data["Id"])
        node_type = node_data["Type"]
        node_types[node_data["Id"]] = node_type
        node_color_map.append(node_colors[node_type])

    # Add connections to the graph
    edge_weights = []
    edges = []

    for conn_id, conn_data in genome["ConnectionGenes"].items():
        if conn_data["Status"] == 0:  # Only add enabled connections
            source = conn_data["Connection"]["Input"]
            target = conn_data["Connection"]["Output"]
            weight = conn_data["Weight"]
            G.add_edge(source, target, weight=weight)
            edges.append((source, target))
            edge_weights.append(weight)

    # Create custom colormap for edges (blue for negative, red for positive)
    colors = [(0, 0, 1), (0.8, 0.8, 0.8), (1, 0, 0)]  # Blue, Light Gray, Red
    cmap = LinearSegmentedColormap.from_list("weight_colormap", colors, N=256)

    # Normalize edge weights for coloring
    if edge_weights:
        vmin = min(edge_weights)
        vmax = max(edge_weights)
    else:
        vmin, vmax = -1, 1  # Default range if no edges exist

    # Group nodes by type
    input_nodes = [n for n, t in node_types.items() if t == 0]
    hidden_nodes = [n for n, t in node_types.items() if t == 1]
    output_nodes = [n for n, t in node_types.items() if t == 2]

    # Create a working copy of the graph to analyze connectivity
    analysis_graph = G.copy()

    # Determine hidden layer distribution based on connectivity
    # We'll assign a layer value to each hidden node
    hidden_layers = {}

    # First, assign layer 0 to input nodes and find max layer (output nodes)
    max_layer = 1  # Start with at least 1 hidden layer by default

    # Find all paths from inputs to outputs to determine maximum path length
    for i_node in input_nodes:
        for o_node in output_nodes:
            try:
                # Get all simple paths (no cycles) from input to output
                for path in nx.all_simple_paths(analysis_graph, i_node, o_node):
                    # Count hidden nodes in the path
                    hidden_count = sum(1 for node in path if node in hidden_nodes)
                    if hidden_count > max_layer:
                        max_layer = hidden_count
            except nx.NetworkXNoPath:
                pass

    # If we couldn't find paths or there's only one hidden layer, just use 1
    if max_layer < 1:
        max_layer = 1

    # Analyze connectivity to assign layers to hidden nodes
    hidden_layers = {}

    # Try to assign layers based on topological sort
    if hidden_nodes:
        try:
            # Get topological generations (nodes grouped by their position in DAG)
            topo_generations = list(nx.topological_generations(analysis_graph))

            # Map each hidden node to its generation
            generation_map = {}
            for gen_idx, generation in enumerate(topo_generations):
                for node in generation:
                    if node in hidden_nodes:
                        generation_map[node] = gen_idx

            # Normalize generations to get layer positions between 0 and 1
            if len(topo_generations) > 2:  # If we have more than input & output layers
                for node, gen in generation_map.items():
                    # Map to range between input and output
                    if gen == 0:
                        hidden_layers[node] = 0.25  # First hidden layer
                    elif gen == len(topo_generations) - 1:
                        hidden_layers[node] = 0.75  # Last hidden layer
                    else:
                        # Distribute remaining layers evenly between 0.25 and 0.75
                        normalized_pos = (gen - 0) / (len(topo_generations) - 2)
                        hidden_layers[node] = 0.25 + (normalized_pos * 0.5)
            else:
                # Simple case: just one hidden layer
                for node in hidden_nodes:
                    hidden_layers[node] = 0.5
        except nx.NetworkXUnfeasible:
            # Graph has cycles, fall back to a simpler approach
            for node in hidden_nodes:
                hidden_layers[node] = 0.5

    # If we couldn't assign layers properly, distribute evenly
    if not hidden_layers and hidden_nodes:
        if max_layer == 1:
            # Just one hidden layer, place all in the middle
            for node in hidden_nodes:
                hidden_layers[node] = 0.5
        else:
            # For each node, find input and output connectivity and estimate position
            for node in hidden_nodes:
                # Count connections from inputs and to outputs
                inputs_connected = sum(
                    1 for i in input_nodes if analysis_graph.has_edge(i, node)
                )
                outputs_connected = sum(
                    1 for o in output_nodes if analysis_graph.has_edge(node, o)
                )

                # Simple heuristic: if more connections from inputs, place closer to inputs
                total = inputs_connected + outputs_connected
                if total > 0:
                    relative_pos = (
                        outputs_connected / total
                    )  # 0 means closer to inputs, 1 closer to outputs
                    hidden_layers[node] = 0.25 + (
                        relative_pos * 0.5
                    )  # Map to 0.25-0.75 range
                else:
                    hidden_layers[node] = 0.5  # Default middle position

    # Create positions dictionary
    pos = {}

    # Position input nodes in the first layer
    if input_nodes:
        input_y_positions = np.linspace(0, 1, len(input_nodes))
        for i, node in enumerate(input_nodes):
            pos[node] = (0, input_y_positions[i])

    # Position output nodes in the last layer
    if output_nodes:
        output_y_positions = np.linspace(0, 1, len(output_nodes))
        for i, node in enumerate(output_nodes):
            pos[node] = (1, output_y_positions[i])

    # Position hidden nodes based on our layer analysis
    # Group hidden nodes by layer for better vertical distribution
    hidden_by_layer = {}
    for node, x_pos in hidden_layers.items():
        # Round to 2 decimals to group similar layers
        rounded_pos = round(x_pos, 2)
        if rounded_pos not in hidden_by_layer:
            hidden_by_layer[rounded_pos] = []
        hidden_by_layer[rounded_pos].append(node)

    # Now position each hidden node with proper spacing
    for x_pos, nodes in hidden_by_layer.items():
        if nodes:
            y_positions = np.linspace(0, 1, len(nodes))
            for i, node in enumerate(nodes):
                pos[node] = (x_pos, y_positions[i])

    # Draw the network
    nx.draw_networkx_nodes(
        G, pos, node_size=500, node_color=node_color_map, alpha=0.8, ax=ax
    )

    # Draw edges with color based on weight
    for i, edge in enumerate(edges):
        nx.draw_networkx_edges(
            G,
            pos,
            edgelist=[edge],
            width=2.0,
            alpha=0.7,
            edge_color=[edge_weights[i]],
            edge_cmap=cmap,
            edge_vmin=vmin,
            edge_vmax=vmax,
            ax=ax,
        )

    # Add labels
    nx.draw_networkx_labels(G, pos, font_size=10, font_weight="bold", ax=ax)

    # Create legend for node types
    legend_elements = [
        mpatches.Patch(color=node_colors[0], label=node_labels[0]),
        mpatches.Patch(color=node_colors[1], label=node_labels[1]),
        mpatches.Patch(color=node_colors[2], label=node_labels[2]),
    ]
    ax.legend(handles=legend_elements, loc="upper right")

    # Add a colorbar for edge weights
    sm = plt.cm.ScalarMappable(cmap=cmap, norm=plt.Normalize(vmin=vmin, vmax=vmax))
    sm.set_array([])
    plt.colorbar(sm, ax=ax, label="Connection Weight")

    # Add title if provided, otherwise use default
    if title:
        ax.set_title(title)
    else:
        ax.set_title(f"Genome Network (Fitness: {genome.get('Fitness', 'N/A')})")

    # Remove axis ticks and labels
    ax.set_xticks([])
    ax.set_yticks([])
    ax.set_frame_on(False)

    plt.tight_layout()
    plt.show()


def plot_fitness_stats(
    generations_range, test_name="test-name", figsize=(12, 6), include_best=True
):
    """
    Plot average, minimum, maximum, and optionally best genome fitness across generations.

    Args:
        generations_range: Range of generations to include (e.g., range(0, 100, 5))
        test_name: The test name prefix in the filenames (default: "test-name")
        figsize: Figure size tuple (width, height)
        include_best: Whether to include the best genome's fitness separately

    Returns:
        The matplotlib figure object
    """
    import numpy as np
    import matplotlib.pyplot as plt
    import json
    import glob
    import os

    # Data storage
    generations = []
    avg_fitness = []
    min_fitness = []
    max_fitness = []
    best_fitness = []

    # Loop through each generation
    for gen in generations_range:
        file_pattern = f"{test_name}_{gen}.json"
        matching_files = glob.glob(file_pattern)

        if not matching_files:
            print(f"Warning: No file found for generation {gen}")
            continue

        # Load data
        with open(matching_files[0], "r") as f:
            data = json.load(f)

        # Collect all genome fitnesses
        all_fitnesses = []

        for species_id, species_data in data["Species"].items():
            for genome_id, genome_data in species_data["Members"].items():
                if "Fitness" in genome_data:
                    all_fitnesses.append(genome_data["Fitness"])

        if not all_fitnesses:
            print(f"Warning: No fitness data found for generation {gen}")
            continue

        # Calculate statistics
        generations.append(gen)
        avg_fitness.append(np.mean(all_fitnesses))
        min_fitness.append(min(all_fitnesses))
        max_fitness.append(max(all_fitnesses))

        # Get best genome fitness if requested
        if include_best and "Best" in data and "Fitness" in data["Best"]:
            best_fitness.append(data["Best"]["Fitness"])

    # Create the plot
    fig, ax = plt.subplots(figsize=figsize)

    # Plot the statistics
    ax.plot(generations, avg_fitness, "b-", label="Average Fitness", linewidth=2)
    ax.plot(
        generations,
        min_fitness,
        "r--",
        label="Minimum Fitness",
        linewidth=1.5,
        alpha=0.7,
    )
    ax.plot(
        generations,
        max_fitness,
        "g--",
        label="Maximum Fitness",
        linewidth=1.5,
        alpha=0.7,
    )

    if include_best and best_fitness:
        ax.plot(
            generations, best_fitness, "k-", label="Best Genome Fitness", linewidth=2.5
        )

    # Add grid and labels
    ax.grid(True, linestyle="--", alpha=0.7)
    ax.set_xlabel("Generation", fontsize=12)
    ax.set_ylabel("Fitness", fontsize=12)
    ax.set_title(f"Fitness Statistics Over Generations - {test_name}", fontsize=14)

    # Add legend
    ax.legend(loc="best", frameon=True, fontsize=10)

    # Format tick labels
    ax.tick_params(axis="both", which="major", labelsize=10)

    # Add annotations for final values
    if generations and False:
        # Annotate final values
        last_gen = generations[-1]
        if avg_fitness:
            ax.annotate(
                f"{avg_fitness[-1]:.2f}",
                xy=(last_gen, avg_fitness[-1]),
                xytext=(5, 0),
                textcoords="offset points",
                fontsize=9,
            )
        if max_fitness:
            ax.annotate(
                f"{max_fitness[-1]:.2f}",
                xy=(last_gen, max_fitness[-1]),
                xytext=(5, 0),
                textcoords="offset points",
                fontsize=9,
            )
        if include_best and best_fitness:
            ax.annotate(
                f"{best_fitness[-1]:.2f}",
                xy=(last_gen, best_fitness[-1]),
                xytext=(5, 0),
                textcoords="offset points",
                fontsize=9,
                fontweight="bold",
            )

    plt.tight_layout()
    plt.show()


def plot_species_count(
    generations_range, test_name="test-name", figsize=(10, 6), moving_avg_window=None
):
    """
    Plot the number of species across generations.

    Args:
        generations_range: Range of generations to include (e.g., range(0, 100, 5))
        test_name: The test name prefix in the filenames (default: "test-name")
        figsize: Figure size tuple (width, height)
        moving_avg_window: If provided, adds a moving average line with the specified window size

    Returns:
        The matplotlib figure object
    """
    import matplotlib.pyplot as plt
    import json
    import glob
    import numpy as np

    # Data storage
    generations = []
    species_counts = []

    # Loop through each generation
    for gen in generations_range:
        file_pattern = f"{test_name}_{gen}.json"
        matching_files = glob.glob(file_pattern)

        if not matching_files:
            print(f"Warning: No file found for generation {gen}")
            continue

        # Load data
        with open(matching_files[0], "r") as f:
            data = json.load(f)

        # Count species
        if "Species" in data:
            species_count = len(data["Species"])
            generations.append(gen)
            species_counts.append(species_count)
        else:
            print(f"Warning: No species data found for generation {gen}")

    # Create the plot
    fig, ax = plt.subplots(figsize=figsize)

    # Plot species count
    ax.plot(
        generations,
        species_counts,
        "b-o",
        label="Number of Species",
        linewidth=2,
        markersize=5,
    )

    # Add moving average if requested
    if moving_avg_window is not None and len(species_counts) > moving_avg_window:
        # Calculate moving average
        moving_avg = []
        for i in range(len(species_counts) - moving_avg_window + 1):
            window_avg = np.mean(species_counts[i : i + moving_avg_window])
            moving_avg.append(window_avg)

        # Plot moving average
        ma_x = generations[moving_avg_window - 1 :]
        ax.plot(
            ma_x,
            moving_avg,
            "r--",
            label=f"{moving_avg_window}-Gen Moving Avg",
            linewidth=2,
            alpha=0.8,
        )

    # Add grid and labels
    ax.grid(True, linestyle="--", alpha=0.7)
    ax.set_xlabel("Generation", fontsize=12)
    ax.set_ylabel("Number of Species", fontsize=12)
    ax.set_title(f"Species Count Over Generations - {test_name}", fontsize=14)

    # Set y-axis to start from 0
    ax.set_ylim(bottom=0)

    # Set integer ticks for y-axis
    from matplotlib.ticker import MaxNLocator

    ax.yaxis.set_major_locator(MaxNLocator(integer=True))

    # Add legend
    ax.legend(loc="best", frameon=True)

    # Add annotations for interesting points
    if generations:
        # Annotate first and last points
        ax.annotate(
            f"{species_counts[0]}",
            xy=(generations[0], species_counts[0]),
            xytext=(0, 5),
            textcoords="offset points",
            ha="center",
            fontsize=9,
        )

        ax.annotate(
            f"{species_counts[-1]}",
            xy=(generations[-1], species_counts[-1]),
            xytext=(0, 5),
            textcoords="offset points",
            ha="center",
            fontsize=9,
        )

        # Find and annotate maximum point
        max_idx = np.argmax(species_counts)
        if (
            species_counts[max_idx] > species_counts[0]
            and species_counts[max_idx] > species_counts[-1]
        ):
            ax.annotate(
                f"{species_counts[max_idx]}",
                xy=(generations[max_idx], species_counts[max_idx]),
                xytext=(0, 5),
                textcoords="offset points",
                ha="center",
                fontsize=9,
            )

    plt.tight_layout()
    plt.show()
