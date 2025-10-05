/**
 * Interface compartilhada para coordenadas geográficas
 */
export interface Coordenadas {
  latitude: number | null;
  longitude: number | null;
}

/**
 * Interface para ponto geográfico com informações adicionais
 */
export interface PontoGeografico extends Coordenadas {
  altitude?: number | null;
  precisao?: number | null;
  timestamp?: Date;
}

/**
 * Interface para bounds/limites geográficos
 */
export interface LimitesGeograficos {
  norte: number;
  sul: number;
  leste: number;
  oeste: number;
}

/**
 * Interface para resultado de cálculo de distância
 */
export interface DistanciaGeografica {
  distanciaKm: number;
  distanciaM: number;
  origem: Coordenadas;
  destino: Coordenadas;
}

/**
 * Constantes geográficas úteis
 */
export const CONSTANTES_GEOGRAFICAS = {
  /**
   * Limites aproximados do território brasileiro
   */
  BRASIL_BOUNDS: {
    norte: 5.27438888,
    sul: -33.75118944,
    leste: -28.63525,
    oeste: -73.98283055
  } as LimitesGeograficos,

  /**
   * Centro geográfico aproximado do Brasil
   */
  BRASIL_CENTRO: {
    latitude: -14.235,
    longitude: -51.9253
  } as Coordenadas,

  /**
   * Raio da Terra em quilômetros
   */
  RAIO_TERRA_KM: 6371,

  /**
   * Conversão de graus para radianos
   */
  GRAUS_PARA_RADIANOS: Math.PI / 180
} as const;