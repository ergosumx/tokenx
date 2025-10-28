#!/usr/bin/env python3
"""Patch Nougat decoder ONNX graphs to insert Identity nodes for outer-scope outputs."""

from __future__ import annotations

import sys
from pathlib import Path

import onnx
from onnx import AttributeProto, helper


def patch_graph(graph: onnx.GraphProto, is_subgraph: bool) -> bool:
    """Recursively patch the supplied graph and its subgraphs."""
    modified = False

    for node in graph.node:
        for attribute in node.attribute:
            if attribute.type == AttributeProto.AttributeType.GRAPH and attribute.g is not None:
                if patch_graph(attribute.g, True):
                    modified = True
            elif attribute.type == AttributeProto.AttributeType.GRAPHS:
                for subgraph in attribute.graphs:
                    if patch_graph(subgraph, True):
                        modified = True

    if not is_subgraph:
        return modified

    produced_names: set[str] = set()

    for node in graph.node:
        for output in node.output:
            if output:
                produced_names.add(output)

    for initializer in graph.initializer:
        if initializer.name:
            produced_names.add(initializer.name)

    for index, output in enumerate(graph.output):
        name = output.name
        if not name or name.endswith("__outer_identity"):
            continue

        if name in produced_names:
            continue

        identity_name = f"{name}__outer_identity"
        identity_node = helper.make_node(
            "Identity",
            inputs=[name],
            outputs=[identity_name],
            name=f"outer_identity_patch_{len(graph.node)}_{index}",
        )

        graph.node.extend([identity_node])
        output.name = identity_name
        produced_names.add(identity_name)

        if output.type is not None:
            value_info = helper.ValueInfoProto()
            value_info.CopyFrom(output)
            value_info.name = identity_name
            graph.value_info.extend([value_info])
        modified = True

    return modified


def patch_model(source: Path, destination: Path) -> None:
    model = onnx.load(source)
    if patch_graph(model.graph, False):
        onnx.save(model, destination)
        print(f"Patched model written to {destination}")
    else:
        print("No outer-scope outputs detected; copying original model.")
        destination.write_bytes(source.read_bytes())


def main() -> int:
    if len(sys.argv) != 3:
        print("Usage: patch_nougat_decoder.py <source> <destination>")
        return 1

    source = Path(sys.argv[1]).resolve()
    destination = Path(sys.argv[2]).resolve()

    if not source.exists():
        print(f"Source model not found: {source}")
        return 2

    destination.parent.mkdir(parents=True, exist_ok=True)

    try:
        patch_model(source, destination)
    except Exception as exc:  # noqa: BLE001 - surface full error for diagnostics
        print(f"Failed to patch decoder model: {exc}")
        return 3

    return 0


if __name__ == "__main__":
    sys.exit(main())
