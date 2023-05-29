namespace LXIntegratedNavigation.Shared.Filters;

public class KalmanFilter
{
    public Vector X { get; private set; }
    public Matrix P { get; private set; }
    public Matrix Q { get; private set; }
    public KalmanFilter(Vector initX, Matrix initP, Matrix initQ)
    {
        X = initX;
        P = initP;
        Q = initQ;
    }
    public (Vector X, Matrix P) Solve(Matrix Gamma_kSub1, Matrix Phi_kSub1Tok, (Matrix H_k, Vector Z_k, Matrix R_k)? measurement = null)
    {
        (Vector X_kSub1, Matrix P_kSub1, Matrix Q_kSub1) = (X, P, Q);
        var X_kSub1Tok = Phi_kSub1Tok * X_kSub1;
        var P_kSub1Tok = Phi_kSub1Tok * P_kSub1 * Phi_kSub1Tok.Transpose() + Gamma_kSub1 * Q_kSub1 * Gamma_kSub1.Transpose();
        if (!measurement.HasValue)
        {
            (X, P) = (X_kSub1Tok, P_kSub1Tok);
            return (X, P);
        }
        (Matrix H_k, Vector Z_k, Matrix R_k) = measurement.Value;
        var Ht_k = H_k.Transpose();
        var K_k = P_kSub1Tok * Ht_k * (H_k * P_kSub1Tok * Ht_k + R_k).Inverse();
        var X_k = X_kSub1Tok + K_k * (Z_k - H_k * X_kSub1Tok);
        var K_kH_k = K_k * H_k;
        var I = Matrix.Identity(K_kH_k.RowCount);
        var P_k = (I - K_kH_k) * P_kSub1Tok * (I - K_kH_k).Transpose() + K_k * R_k * K_k.Transpose();
        (X, P) = (X_k, P_k);
        return (X, P);
    }
}
