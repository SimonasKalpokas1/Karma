/*
Auto-generated by: https://github.com/pmndrs/gltfjsx
*/

import React, { useRef } from 'react'
import { useGLTF } from '@react-three/drei'

export default function Flower(props) {
  const group = useRef()
  const { nodes, materials } = useGLTF('/Desert marigold.glb')
  return (
    <group ref={group} {...props} dispose={null} scale={0.2}>
      <mesh
        castShadow
        receiveShadow
        geometry={nodes.DesertMarigold_mesh.geometry}
        material={materials.DesertMarigold_mat}
      />
    </group>
  )
}

useGLTF.preload('/Desert marigold.glb')