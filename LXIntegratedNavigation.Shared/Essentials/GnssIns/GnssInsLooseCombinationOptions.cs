namespace LXIntegratedNavigation.Shared.Essentials;

public record class GnssInsLooseCombinationOptions(Vector L_Gnss, Matrix C_b_r, Angle StdInitialPhin, Angle StdInitialPhie, Angle StdInitialPhid, double Arw, double Vrw, double Sigma_ab, double Sigma_as, double Sigma_gb, double Sigma_gs, double StdInitialRn, double StdInitialRe, double StdInitialRd, double StdInitialVn, double StdInitialVe, double StdInitialVd, double Tab, double Tas, double Tgb, double Tgs);
