#!/usr/bin/env python3
import sys
import lief

def find_assembly_resource(pe):
    """Cherche une ressource de type ASSEMBLY, retourne (type_id, name_id, lang_id, resource_obj)."""
    if not pe.resources:
        return None

    for type_entry in pe.resources.childs:
        # Vérifier si le type s'appelle ASSEMBLY
        if type_entry.name and type_entry.name.upper() == "ASSEMBLY":
            type_id = type_entry.id

            # Descend au niveau ID (ex : 101)
            for id_entry in type_entry.childs:
                name_id = id_entry.id

                # Langue (ex : 1033)
                for lang_entry in id_entry.childs:
                    lang_id = lang_entry.id
                    return (type_id, name_id, lang_id, lang_entry)

    return None


def main():
	if len(sys.argv) != 4:
		print(f"Usage: {sys.argv[0]} <input.exe> <new_assembly.bin> <output.exe>")
		sys.exit(1)

	input_pe = sys.argv[1]
	new_data_path = sys.argv[2]
	output_pe = sys.argv[3]

	print("[+] Loading PE...")
	pe = lief.parse(input_pe)

	# Charger le payload binaire
	with open(new_data_path, "rb") as f:
		new_data = f.read()

	# Trouver la ressource automatiquement
	print("[+] Searching for ASSEMBLY resource...")
	res_info = find_assembly_resource(pe)

	if not res_info:
		print("[!] No ASSEMBLY resource found!")
		sys.exit(1)

	type_id, name_id, lang_id, res_obj = res_info
	print(f"[+] Found ASSEMBLY resource: type={type_id}, name={name_id}, lang={lang_id}")

	# Remplacement
	print("[+] Replacing resource content...")
	res_obj.content = new_data  # IMPORTANT: bytes only

	# Reconstruction PE
	print("[+] Rebuilding PE...")
	config = lief.PE.Builder.config_t()  # pas de build_resources
	builder = lief.PE.Builder(pe, config)
	builder.build()
	builder.write(output_pe)

	print(f"[?] Done! Output written to: {output_pe}")


if __name__ == "__main__":
    main()
