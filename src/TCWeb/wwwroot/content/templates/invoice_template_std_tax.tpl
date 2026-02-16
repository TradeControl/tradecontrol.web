<style>
    .tc-tax {
        width: 100%;
        border-collapse: collapse;
        border: 1px solid #e5e7eb;
        border-radius: 10px;
        overflow: hidden;
    }

    .tc-tax th,
    .tc-tax td {
        padding: 10px 10px;
        border-bottom: 1px solid #e5e7eb;
        vertical-align: top;
    }

    .tc-tax thead th {
        background: #f8fafc;
        color: #6b7280;
        font-weight: 600;
        font-size: 12px;
        text-transform: uppercase;
        letter-spacing: 0.06em;
    }

    .tc-tax .num {
        text-align: right;
        white-space: nowrap;
    }
</style>

<table class="tc-tax" role="table" aria-label="VAT summary">
    <thead>
        <tr>
            <th style="width: 20%;">[Col_TaxCode]</th>
            <th class="num" style="width: 30%;">[Col_InvoiceValueTotal]</th>
            <th class="num" style="width: 30%;">[Col_TaxValueTotal]</th>
            <th class="num" style="width: 20%;">[Col_TaxRate]</th>
        </tr>
    </thead>

    <tbody>
        <!--ITEM-->
        <tr>
            <td>[TaxCode]</td>
            <td class="num">[InvoiceValueTotal]</td>
            <td class="num">[TaxValueTotal]</td>
            <td class="num">[TaxRate]</td>
        </tr>
        <!--/ITEM-->
        [Items]
    </tbody>
</table>
