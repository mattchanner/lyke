export interface KibbeStyleGuide {
  silhouettes: string[];
  colours: string[];
  fabrics: string[];
  avoid: string[];
  celebrities: string[];
}

export const KIBBE_STYLE_GUIDES: Record<string, KibbeStyleGuide> = {
  Dramatic: {
    silhouettes: ['Sleek column dresses', 'Elongated blazers', 'Minimal, sharp tailoring'],
    colours: ['Bold monochromes', 'High-contrast black & white', 'Rich jewel tones'],
    fabrics: ['Structured crepe', 'Crisp suiting fabric', 'Leather & faux leather'],
    avoid: ['Ruffles & frills', 'Delicate floral prints', 'Oversized shapeless layers'],
    celebrities: ['Tilda Swinton', 'Cate Blanchett', 'Joan Crawford'],
  },
  Natural: {
    silhouettes: ['Relaxed shirt dresses', 'Wide-leg trousers', 'Unfussy wrap styles'],
    colours: ['Earth tones', 'Warm neutrals', 'Muted sage & terracotta'],
    fabrics: ['Linen', 'Cotton', 'Woven wool', 'Denim'],
    avoid: ['Sleek synthetic fabrics', 'Fussy embellishments', 'Very tight tailoring'],
    celebrities: ['Jennifer Aniston', 'Julia Roberts', 'Cameron Diaz'],
  },
  Classic: {
    silhouettes: ['Tailored blazer and trouser', 'Sheath dress', 'A-line skirt'],
    colours: ['Navy', 'Camel', 'Burgundy', 'Soft white'],
    fabrics: ['Cashmere', 'Silk', 'Mid-weight wool'],
    avoid: ['Trendy statement pieces', 'Extreme proportions', 'Costume-y details'],
    celebrities: ['Grace Kelly', 'Audrey Hepburn', 'Kate Middleton'],
  },
  Gamine: {
    silhouettes: ['Cropped jackets', 'Playful A-line mini', 'Geometric cuts'],
    colours: ['Bold graphic contrast', 'Crisp black & white', 'Bright accent colours'],
    fabrics: ['Crisp cotton', 'Jersey knit', 'Lightweight denim'],
    avoid: ['Overly flowing fabrics', 'Very long hemlines', 'Heavy draped styles'],
    celebrities: ['Audrey Hepburn (Gamine side)', 'Winona Ryder', 'Zooey Deschanel'],
  },
  Romantic: {
    silhouettes: ['Fitted bodice with flared skirt', 'Draped wrap dress', 'Soft cowl necklines'],
    colours: ['Soft pastels', 'Blush & rose', 'Warm ivory'],
    fabrics: ['Chiffon', 'Lace', 'Soft velvet', 'Silk satin'],
    avoid: ['Sharp angular tailoring', 'Boxy silhouettes', 'Heavy structured fabrics'],
    celebrities: ['Marilyn Monroe', 'Sophia Loren', 'Nigella Lawson'],
  },
};
